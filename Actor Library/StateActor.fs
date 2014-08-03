(*
 * The MIT License (MIT)
 *
 * Copyright (c) 2014 Andrew B. Johnson
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *)

namespace Actors

open Microsoft.FSharp.Control

[<AbstractClass>]
type StateActor<'a, 'state>(initialState : 'state) as this =
   (* Private methods *)
   let actorLoop state (inbox : MailboxProcessor<'a>) =
      let rec looper state =
         async {
            let! message = inbox.Receive()

            if this.IsShutdownMessage message then
               return this.ProcessShutdown state
            else
               let newState = this.ProcessMessage state message
               return! looper newState
         }
      
      looper state

   (* Private fields *)
   let mailbox = MailboxProcessor.Start <| actorLoop initialState

   (* Public methods *)
   /// <summary>
   /// Protected call. Process any shutdown tasks prior to a full shutdown
   /// </summary>
   abstract member ProcessShutdown : 'state -> unit

   /// <summary>
   /// Protected call. Process a message, given the state and the message
   /// </summary>
   abstract member ProcessMessage : 'state -> 'a -> 'state

   /// <summary>
   /// Protected call. Determines whether or not a message passed is instructing
   /// this actor to shutdown
   /// </summary>
   abstract member IsShutdownMessage : 'a -> bool

   interface IActor<'a> with
      member this.Post msg = mailbox.Post msg
   end