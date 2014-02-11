Calculator Example for Rainer Hessmer's C# port of
Samek's Quantum Hierarchical State Machine.

Author: David Shields (david@shields.net)
This code is adapted from Samek's C example.
See the following site for the statechart:
http://www.quantum-leaps.com/cookbook/recipes.htm


NOTE: 

The code for the state machine (see file CalcHsm.cs) leverages a pre-compiler define.
If STATIC_TRANS is defined for a given build configuration then the calculator state
machine will use static transition that also support the derivation of more complex 
state machines from the given state machine without losing the ability to leverage
static transitions. (See the application notes of Rainer Hessmer's port of the
Quantum Hierarchical State Machine for more details.)

 
References:
Practical Statecharts in C/C++; Quantum Programming for Embedded Systems
Author: Miro Samek, Ph.D.
http://www.quantum-leaps.com/book.htm

Rainer Hessmer, Ph.D. (rainer@hessmer.org)
http://www.hessmer.org/dev/qhsm/