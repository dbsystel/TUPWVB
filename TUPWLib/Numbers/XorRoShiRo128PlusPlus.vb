'
' SPDX-FileCopyrightText: 2022 DB Systel GmbH
'
' SPDX-License-Identifier: Apache-2.0
'
' Licensed under the Apache License, Version 2.0 (the "License")
' You may not use this file except in compliance with the License.
'
' You may obtain a copy of the License at
'
'     http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
'
' Author: Frank Schwab, DB Systel GmbH
'
' Version: 1.0.1
'
' Change history:
'    2020-05-06: V1.0.0: Created.
'    2020-05-25: V1.0.1: Added exceptions for constructor with array.
'

Option Strict On
Option Explicit On

Imports System.Numerics


''' <summary>
''' Xoroshiro128plusplus pseudo-random number generator
''' </summary>
''' <remarks>
''' It is derived from the <a href="http://prng.di.unimi.it/xoroshiro128plusplus.c">C source code</a>.
''' </remarks>
Public Class XorRoShiRo128PlusPlus : Inherits SimplePseudoRandomNumberGenerator
#Region "Instance variables"
   '******************************************************************
   ' Instance variables
   '******************************************************************

   '
   ' The state variables
   '
   Private m_State0 As Long
   Private m_State1 As Long
#End Region

#Region "Constructor"
   '******************************************************************
   ' Constructors
   '******************************************************************

   ''' <summary>
   ''' Constructor for Xoroshiro128plusplus with seed.
   ''' </summary>
   ''' <param name="seed">Initial seed.</param>
   Public Sub New(seed As Long)
      InitializeState(seed)
   End Sub

   ''' <summary>
   ''' Constructor for Xoroshiro128plusplus with seed array.
   ''' </summary>
   ''' <param name="seed">Initial seed array.</param>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="seed"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="ArgumentOutOfRangeException">Thrown if length of <paramref name="seed"/> is not 2.</exception>
   Public Sub New(seed As Long())
      If seed Is Nothing Then _
         Throw New ArgumentNullException(NameOf(seed))

      If seed.Length <> 2 Then _
         Throw New ArgumentOutOfRangeException(NameOf(seed))

      m_State0 = seed(0)
      m_State1 = seed(1)
   End Sub

   ''' <summary>
   ''' Constructor for Xoroshiro128plusplus with two seed values.
   ''' </summary>
   ''' <param name="seed0">Initial seed 1.</param>
   ''' <param name="seed1">Initial seed 2.</param>
   Public Sub New(seed0 As Long, seed1 As Long)
      m_State0 = seed0
      m_State1 = seed1
   End Sub
#End Region

#Region "Public methods"
   '******************************************************************
   ' Public methods
   '******************************************************************

   ''' <summary>
   ''' Get a pseudo-random <see cref="Long"/> value.
   ''' </summary>
   ''' <returns>Pseudo-random value.</returns>
   Public Overrides Function GetLong() As Long
      Dim s0 As Long = m_State0
      Dim s1 As Long = m_State1

      Dim result As Long = GetNextLong(s0, s1)

      UpdateState(s0, s1)

      Return result
   End Function
#End Region

#Region "Private methods"
   '******************************************************************
   ' Private methods
   '******************************************************************

   ''' <summary>
   ''' Initialize pseudo-random number generator state.
   ''' </summary>
   ''' <param name="seed"></param>
   Private Sub InitializeState(seed As Long)
      Dim sm64 As New SplitMix64(seed)

      m_State0 = sm64.GetLong()
      m_State1 = sm64.GetLong()
   End Sub

   ''' <summary>
   ''' Compute next result.
   ''' </summary>
   ''' <remarks>
   ''' This method only exists because Visual Basic can not deal with overflows.
   ''' </remarks>
   ''' <param name="s0">Current state 0.</param>
   ''' <param name="s1">Current state 1.</param>
   ''' <returns>Next pseudo-random number.</returns>
   Private Shared Function GetNextLong(s0 As Long, s1 As Long) As Long
      Dim ts As New BigInteger(s0)

      ts += s1
      ts = BitwiseIntegerConverter.GetLowLongOfBigInteger(ts)

      ts = BitManipulationHelper.RotateLeftForLong(ts, 17) + s0

      ts = BitwiseIntegerConverter.GetLowLongOfBigInteger(ts)

      Return CLng(ts)
   End Function

   ''' <summary>
   ''' Update state of pseudo-random number. 
   ''' </summary>
   ''' <remarks>
   ''' This method only exists because Visual Basic can not deal with overflows.
   ''' </remarks>
   ''' <param name="s0">Current state 0.</param>
   ''' <param name="s1">Current state 1.</param>
   Private Sub UpdateState(s0 As Long, s1 As Long)
      s1 = s1 Xor s0

      m_State0 = BitManipulationHelper.RotateRight(s0, 15) Xor s1 Xor (s1 << 21)
      m_State1 = BitManipulationHelper.RotateLeft(s1, 28)
   End Sub
#End Region
End Class
