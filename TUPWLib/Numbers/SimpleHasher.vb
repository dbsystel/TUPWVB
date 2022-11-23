'
' SPDX-FileCopyrightText: 2022 DB Systel GmbH
'
' SPDX-License-Identifier: Apache-2.0
'
' Licensed under the Apache License, Version 2.0 (the "License");
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
' Version: 1.1.0
'
' Change history:
'    2020-04-21: V1.0.0: Created.
'    2020-07-13: V1.0.1: Enlarge value range.
'    2020-08-11: V1.1.0: Restrict hash value calculation to 32 bits.
'
' This hasher is built like the Xoroshiro64** random number generator by 
' David Blackman and Sebastiano Vigna. Here is their copyright notice
' from the web site http://prng.di.unimi.it/xoroshiro64starstar.c:
'
' Written in 2018 by David Blackman and Sebastiano Vigna (vigna@acm.org)
' To the extent possible under law, the author has dedicated all copyright
' and related and neighboring rights to this software to the public domain
' worldwide.This software is distributed without any warranty.
'
' See <http://creativecommons.org/publicdomain/zero/1.0/>.
'

Option Strict On
Option Explicit On

''' <summary>
''' Simple hasher for various data types.
''' </summary>
Public Class SimpleHasher
#Region "Private constants"
#Region "Bit mask constants"
   Private Const MAX_32_BIT_UINTEGER_AS_ULONG As ULong = &HFFFFFFFFUL
#End Region

#Region "Shift values"
   Private Const BYTE_SHIFT_VALUE As Integer = 11
   Private Const SHORT_SHIFT_VALUE As Integer = 7
#End Region

#Region "Calculation constants"
   Private Const MULTIPLICATOR_FOR_HASH As ULong = &H9E3779BBUL
#End Region
#End Region

#Region "Instance variables"
   ''' <summary>
   ''' Storage for the seeds when class is instantiated for use with the <see cref="Reset()"/> method.
   ''' </summary>
   Private ReadOnly m_Seed As UInteger() = New UInteger(0 To 1) {}

   ''' <summary>
   ''' Hash state.
   ''' </summary>
   Private ReadOnly m_HashState As UInteger() = New UInteger(0 To 1) {}
#End Region

#Region "Constructor"
   ''' <summary>
   ''' Create new <see cref="SimpleHasher"/>.
   ''' </summary>
   ''' <param name="seed1">1. seed value.</param>
   ''' <param name="seed2">2. seed value.</param>
   Public Sub New(seed1 As UInteger, seed2 As UInteger)
      m_Seed(0) = seed1
      m_Seed(1) = seed2

      Reset()
   End Sub
#End Region

#Region "Public methods"
   ''' <summary>
   ''' Reset hash state to seeds.
   ''' </summary>
   Public Sub Reset()
      m_HashState(0) = m_Seed(0)
      m_HashState(1) = m_Seed(1)
   End Sub


#Region "UpdateHash methods"
   ''' <summary>
   ''' Update hash with an <see cref="SByte"/> value.
   ''' </summary>
   ''' <param name="aValue">Value to add to hash.</param>
   Public Sub UpdateHash(aValue As SByte)
      Dim safeValue As Byte

      safeValue = BitwiseIntegerConverter.AsUnsignedByte(aValue)

      UpdateHashForValue(safeValue << BYTE_SHIFT_VALUE)
   End Sub

   ''' <summary>
   ''' Update hash with a <see cref="Byte"/> value.
   ''' </summary>
   ''' <param name="aValue">Value to add to hash.</param>
   Public Sub UpdateHash(aValue As Byte)
      UpdateHashForValue(aValue << BYTE_SHIFT_VALUE)
   End Sub

   ''' <summary>
   ''' Update hash with a <see cref="Short"/> value.
   ''' </summary>
   ''' <param name="aValue">Value to add to hash.</param>
   Public Sub UpdateHash(aValue As Short)
      Dim safeValue As UShort

      safeValue = BitwiseIntegerConverter.AsUnsignedShort(aValue)

      UpdateHashForValue(safeValue << SHORT_SHIFT_VALUE)
   End Sub

   ''' <summary>
   ''' Update hash with an <see cref="UShort"/> value.
   ''' </summary>
   ''' <param name="aValue">Value to add to hash.</param>
   Public Sub UpdateHash(aValue As UShort)
      UpdateHashForValue(aValue << SHORT_SHIFT_VALUE)
   End Sub

   ''' <summary>
   ''' Update hash with an <see cref="Integer"/> value.
   ''' </summary>
   ''' <param name="aValue">Value to add to hash.</param>
   Public Sub UpdateHash(aValue As Integer)
      Dim safeValue As UInteger

      safeValue = BitwiseIntegerConverter.AsUnsignedInteger(aValue)

      UpdateHashForValue(safeValue)
   End Sub

   ''' <summary>
   ''' Update hash with an <see cref="UInteger"/> value.
   ''' </summary>
   ''' <param name="aValue">Value to add to hash.</param>
   Public Sub UpdateHash(aValue As UInteger)
      UpdateHashForValue(aValue)
   End Sub
#End Region

   ''' <summary>
   ''' Calculate the hash value.
   ''' </summary>
   ''' <returns>Hash value.</returns>
   Public Function GetHashValue() As Integer
      ' Maximum possible value is 9E37 79BA 61C8 8645
      Dim result As ULong = m_HashState(0) * MULTIPLICATOR_FOR_HASH

      result = (result << 5) Or (result >> 27)           ' This is a 32-bit rotate left on a 64-bit integer variable which is not implemented in BitManipulationHelper

      result = result And MAX_32_BIT_UINTEGER_AS_ULONG   ' Only use the lower 32 bits

      ' Maximum possible value is 4 FFFF FFFB
      result *= 5UL

      Return BitwiseIntegerConverter.AsSignedInteger(result)
   End Function
#End Region

#Region "Private methods"
   ''' <summary>
   ''' Update hash for a value.
   ''' </summary>
   ''' <param name="aValue">Value to add to hash.</param>
   Private Sub UpdateHashForValue(aValue As UInteger)
      Dim s0 As UInteger = m_HashState(0)
      Dim s1 As UInteger = m_HashState(1)

      s1 = s1 Xor s0

      s0 = s0 Xor aValue   ' Here the new value is added in

      m_HashState(0) = BitManipulationHelper.RotateRight(s0, 6) Xor s1 Xor (s1 << 9)
      m_HashState(1) = BitManipulationHelper.RotateLeft(s1, 13)
   End Sub
#End Region
End Class
