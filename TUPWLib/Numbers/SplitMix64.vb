'
' SPDX-FileCopyrightText: 2020 DB Systel GmbH
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
' Version: 1.0.0
'
' Change history:
'    2020-05-06: V1.0.0: Created.
'

Imports System.Numerics

''' <summary>
''' Splitmix64 pseudo-random number generator.
''' </summary>
''' <remarks>
''' It is derived from the <a href="http://xoroshiro.di.unimi.it/splitmix64.c">C source code</a>.
''' </remarks>
Public Class SplitMix64 : Inherits SimplePseudoRandomNumberGenerator
#Region "Instance variables"
   '******************************************************************
   ' Instance variables
   '******************************************************************

   ''' <summary>
   ''' State.
   ''' </summary>
   Private m_State As Long
#End Region

#Region "Constructor"
   '******************************************************************
   ' Constructors
   '******************************************************************

   ''' <summary>
   ''' Create new instance.
   ''' </summary>
   ''' <param name="seed">Seed to use for this instance.</param>
   Public Sub New(seed As Long)
      m_State = seed
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
      Dim z As BigInteger = UpdateState()

      z = (z Xor BitManipulationHelper.UnsignedShiftRightForLong(z, 30)) * &HBF58476D1CE4E5B9L
      z = BitwiseIntegerConverter.GetLowLongOfBigInteger(z)
      z = (z Xor BitManipulationHelper.UnsignedShiftRightForLong(z, 27)) * &H94D049BB133111EBL
      z = BitwiseIntegerConverter.GetLowLongOfBigInteger(z)

      Dim result As Long = CLng(z)

      Return result Xor BitManipulationHelper.UnsignedShiftRight(result, 31)
   End Function
#End Region

#Region "Private methods"
   ''' <summary>
   ''' Update state of pseudo-random number. 
   ''' </summary>
   ''' <remarks>
   ''' This method only exists because Visual Basic can not deal with overflows.
   ''' </remarks>
   ''' <returns>New state.</returns>
   Private Function UpdateState() As BigInteger
      Dim ts As New BigInteger(m_State)

      ts += &H9E3779B97F4A7C15L
      ts = BitwiseIntegerConverter.GetLowLongOfBigInteger(ts)

      m_State = CLng(ts)

      Return ts
   End Function
#End Region
End Class
