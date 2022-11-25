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
' Version: 1.1.1
'
' Change history:
'    2020-05-06: V1.0.0: Created.
'    2020-10-26: V1.0.1: Use array literals.
'    2022-11-22: V1.1.0: Counts are always integers.
'    2022-11-25: V1.1.1: Added method comments.
'

Option Strict On
Option Explicit On

Imports System.Numerics

''' <summary>
''' Bit manipulation helper
''' </summary>
''' <remarks>
''' This class exists because .Net lacks a lot of bit manipulations that are necessary for programming.
''' </remarks>
Public NotInheritable Class BitManipulationHelper
#Region "Private constants"
   Private Const BIT_SIZE_FOR_BYTE As Integer = 8
   Private Const BIT_SIZE_FOR_SHORT As Integer = 16
   Private Const BIT_SIZE_FOR_INTEGER As Integer = 32
   Private Const BIT_SIZE_FOR_LONG As Integer = 64

   Private Const SHIFT_MASK_FOR_BYTE As Integer = BIT_SIZE_FOR_BYTE - 1
   Private Const SHIFT_MASK_FOR_SHORT As Integer = BIT_SIZE_FOR_SHORT - 1
   Private Const SHIFT_MASK_FOR_INTEGER As Integer = BIT_SIZE_FOR_INTEGER - 1
   Private Const SHIFT_MASK_FOR_LONG As Integer = BIT_SIZE_FOR_LONG - 1

   Private Shared ReadOnly MASK_FOR_SIGNED_BYTE_RIGHT_SHIFT As SByte() = New SByte() {
   -1S, &H7FS, &H3FS, &H1FS, &HFS, &H7S, &H3S, &H1S
   }

   Private Shared ReadOnly MASK_FOR_SHORT_RIGHT_SHIFT As Short() = {
   &HFFFFS, &H7FFFS, &H3FFFS, &H1FFFS,
   &HFFFS, &H7FFS, &H3FFS, &H1FFS,
   &HFFS, &H7FS, &H3FS, &H1FS,
   &HFS, &H7S, &H3S, &H1S
   }

   Private Shared ReadOnly MASK_FOR_INTEGER_RIGHT_SHIFT As Integer() = {
   &HFFFFFFFFI, &H7FFFFFFFI, &H3FFFFFFFI, &H1FFFFFFFI,
   &HFFFFFFFI, &H7FFFFFFI, &H3FFFFFFI, &H1FFFFFFI,
   &HFFFFFFI, &H7FFFFFI, &H3FFFFFI, &H1FFFFFI,
   &HFFFFFI, &H7FFFFI, &H3FFFFI, &H1FFFFI,
   &HFFFFI, &H7FFFI, &H3FFFI, &H1FFFI,
   &HFFFI, &H7FFI, &H3FFI, &H1FFI,
   &HFFI, &H7FI, &H3FI, &H1FI,
   &HFI, &H7I, &H3I, &H1I
   }

   Private Shared ReadOnly MASK_FOR_LONG_RIGHT_SHIFT As Long() = {
   &HFFFFFFFFFFFFFFFFL, &H7FFFFFFFFFFFFFFFL, &H3FFFFFFFFFFFFFFFL, &H1FFFFFFFFFFFFFFFL,
   &HFFFFFFFFFFFFFFFL, &H7FFFFFFFFFFFFFFL, &H3FFFFFFFFFFFFFFL, &H1FFFFFFFFFFFFFFL,
   &HFFFFFFFFFFFFFFL, &H7FFFFFFFFFFFFFL, &H3FFFFFFFFFFFFFL, &H1FFFFFFFFFFFFFL,
   &HFFFFFFFFFFFFFL, &H7FFFFFFFFFFFFL, &H3FFFFFFFFFFFFL, &H1FFFFFFFFFFFFL,
   &HFFFFFFFFFFFFL, &H7FFFFFFFFFFFL, &H3FFFFFFFFFFFL, &H1FFFFFFFFFFFL,
   &HFFFFFFFFFFFL, &H7FFFFFFFFFFL, &H3FFFFFFFFFFL, &H1FFFFFFFFFFL,
   &HFFFFFFFFFFL, &H7FFFFFFFFFL, &H3FFFFFFFFFL, &H1FFFFFFFFFL,
   &HFFFFFFFFFL, &H7FFFFFFFFL, &H3FFFFFFFFL, &H1FFFFFFFFL,
   &HFFFFFFFFL, &H7FFFFFFFL, &H3FFFFFFFL, &H1FFFFFFFL,
   &HFFFFFFFL, &H7FFFFFFL, &H3FFFFFFL, &H1FFFFFFL,
   &HFFFFFFL, &H7FFFFFL, &H3FFFFFL, &H1FFFFFL,
   &HFFFFFL, &H7FFFFL, &H3FFFFL, &H1FFFFL,
   &HFFFFL, &H7FFFL, &H3FFFL, &H1FFFL,
   &HFFFL, &H7FFL, &H3FFL, &H1FFL,
   &HFFL, &H7FL, &H3FL, &H1FL,
   &HFL, &H7L, &H3L, &H1L
   }
#End Region

#Region "Public methods"
#Region "Unsigned shift right methods"
   ''' <summary>
   ''' Logical shift right with zero bits filling from the left.
   ''' </summary>
   ''' <param name="aValue">Value to be logically shifted right.</param>
   ''' <param name="shiftValue">Number of bits to shift right.</param>
   ''' <returns><paramref name="aValue"/> shifted right <paramref name="shiftValue"/> bits with zero bit filling from the left.</returns>
   Public Shared Function UnsignedShiftRight(aValue As SByte, shiftValue As Integer) As SByte
      Dim normalizedShiftValue As Integer = shiftValue And SHIFT_MASK_FOR_BYTE

      Return (aValue >> normalizedShiftValue) And MASK_FOR_SIGNED_BYTE_RIGHT_SHIFT(normalizedShiftValue)
   End Function

   ''' <summary>
   ''' Logical shift right with zero bits filling from the left.
   ''' </summary>
   ''' <param name="aValue">Value to be logically shifted right.</param>
   ''' <param name="shiftValue">Number of bits to shift right.</param>
   ''' <returns><paramref name="aValue"/> shifted right <paramref name="shiftValue"/> bits with zero bit filling from the left.</returns>
   Public Shared Function UnsignedShiftRight(aValue As Short, shiftValue As Integer) As Short
      Dim normalizedShiftValue As Integer = shiftValue And SHIFT_MASK_FOR_SHORT

      Return (aValue >> normalizedShiftValue) And MASK_FOR_SHORT_RIGHT_SHIFT(normalizedShiftValue)
   End Function

   ''' <summary>
   ''' Logical shift right with zero bits filling from the left.
   ''' </summary>
   ''' <param name="aValue">Value to be logically shifted right.</param>
   ''' <param name="shiftValue">Number of bits to shift right.</param>
   ''' <returns><paramref name="aValue"/> shifted right <paramref name="shiftValue"/> bits with zero bit filling from the left.</returns>
   Public Shared Function UnsignedShiftRight(aValue As Integer, shiftValue As Integer) As Integer
      Dim normalizedShiftValue As Integer = shiftValue And SHIFT_MASK_FOR_INTEGER

      Return (aValue >> normalizedShiftValue) And MASK_FOR_INTEGER_RIGHT_SHIFT(normalizedShiftValue)
   End Function

   ''' <summary>
   ''' Logical shift right with zero bits filling from the left.
   ''' </summary>
   ''' <param name="aValue">Value to be logically shifted right.</param>
   ''' <param name="shiftValue">Number of bits to shift right.</param>
   ''' <returns><paramref name="aValue"/> shifted right <paramref name="shiftValue"/> bits with zero bit filling from the left.</returns>
   Public Shared Function UnsignedShiftRight(aValue As Long, shiftValue As Integer) As Long
      Dim normalizedShiftValue As Integer = shiftValue And SHIFT_MASK_FOR_LONG

      Return (aValue >> normalizedShiftValue) And MASK_FOR_LONG_RIGHT_SHIFT(normalizedShiftValue)
   End Function

   ''' <summary>
   ''' Logical shift right with zero bits filling from the left.
   ''' </summary>
   ''' <param name="aValue">Value to be logically shifted right.</param>
   ''' <param name="shiftValue">Number of bits to shift right.</param>
   ''' <returns><paramref name="aValue"/> shifted right <paramref name="shiftValue"/> bits with zero bit filling from the left.</returns>
   Public Shared Function UnsignedShiftRightForLong(aValue As BigInteger, shiftValue As Integer) As BigInteger
      Dim normalizedShiftValue As Integer = shiftValue And SHIFT_MASK_FOR_LONG

      Return (aValue >> normalizedShiftValue) And MASK_FOR_LONG_RIGHT_SHIFT(normalizedShiftValue)
   End Function
#End Region

#Region "Rotate methods"
#Region "Rotate left methods"
   ''' <summary>
   ''' Rotate a value to the left.
   ''' </summary>
   ''' <param name="aValue">Value to rotate.</param>
   ''' <param name="rotateValue">Number of bits to rotate.</param>
   ''' <returns><paramref name="aValue"/> rotated left by <paramref name="rotateValue"/> bits.</returns>
   Public Shared Function RotateLeft(aValue As SByte, rotateValue As Integer) As SByte
      Dim normalizedShiftValue As Integer = rotateValue And SHIFT_MASK_FOR_BYTE

      Return (aValue << normalizedShiftValue) Or UnsignedShiftRight(aValue, BIT_SIZE_FOR_BYTE - normalizedShiftValue)
   End Function

   ''' <summary>
   ''' Rotate a value to the left.
   ''' </summary>
   ''' <param name="aValue">Value to rotate.</param>
   ''' <param name="rotateValue">Number of bits to rotate.</param>
   ''' <returns><paramref name="aValue"/> rotated left by <paramref name="rotateValue"/> bits.</returns>
   Public Shared Function RotateLeft(aValue As Byte, rotateValue As Integer) As Byte
      Dim normalizedShiftValue As Integer = rotateValue And SHIFT_MASK_FOR_BYTE

      Return (aValue << normalizedShiftValue) Or (aValue >> (BIT_SIZE_FOR_BYTE - normalizedShiftValue))
   End Function

   ''' <summary>
   ''' Rotate a value to the left.
   ''' </summary>
   ''' <param name="aValue">Value to rotate.</param>
   ''' <param name="rotateValue">Number of bits to rotate.</param>
   ''' <returns><paramref name="aValue"/> rotated left by <paramref name="rotateValue"/> bits.</returns>
   Public Shared Function RotateLeft(aValue As Short, rotateValue As Integer) As Short
      Dim normalizedShiftValue As Integer = rotateValue And SHIFT_MASK_FOR_SHORT

      Return (aValue << normalizedShiftValue) Or UnsignedShiftRight(aValue, BIT_SIZE_FOR_SHORT - normalizedShiftValue)
   End Function

   ''' <summary>
   ''' Rotate a value to the left.
   ''' </summary>
   ''' <param name="aValue">Value to rotate.</param>
   ''' <param name="rotateValue">Number of bits to rotate.</param>
   ''' <returns><paramref name="aValue"/> rotated left by <paramref name="rotateValue"/> bits.</returns>
   Public Shared Function RotateLeft(aValue As UShort, rotateValue As Integer) As UShort
      Dim normalizedShiftValue As Integer = rotateValue And SHIFT_MASK_FOR_SHORT

      Return (aValue << normalizedShiftValue) Or (aValue >> (BIT_SIZE_FOR_SHORT - normalizedShiftValue))
   End Function

   ''' <summary>
   ''' Rotate a value to the left.
   ''' </summary>
   ''' <param name="aValue">Value to rotate.</param>
   ''' <param name="rotateValue">Number of bits to rotate.</param>
   ''' <returns><paramref name="aValue"/> rotated left by <paramref name="rotateValue"/> bits.</returns>
   Public Shared Function RotateLeft(aValue As Integer, rotateValue As Integer) As Integer
      Dim normalizedShiftValue As Integer = rotateValue And SHIFT_MASK_FOR_INTEGER

      Return (aValue << normalizedShiftValue) Or UnsignedShiftRight(aValue, BIT_SIZE_FOR_INTEGER - normalizedShiftValue)
   End Function

   ''' <summary>
   ''' Rotate a value to the left.
   ''' </summary>
   ''' <param name="aValue">Value to rotate.</param>
   ''' <param name="rotateValue">Number of bits to rotate.</param>
   ''' <returns><paramref name="aValue"/> rotated left by <paramref name="rotateValue"/> bits.</returns>
   Public Shared Function RotateLeft(aValue As UInteger, rotateValue As Integer) As UInteger
      Dim normalizedShiftValue As Integer = rotateValue And SHIFT_MASK_FOR_INTEGER

      Return (aValue << normalizedShiftValue) Or (aValue >> (BIT_SIZE_FOR_INTEGER - normalizedShiftValue))
   End Function

   ''' <summary>
   ''' Rotate a value to the left.
   ''' </summary>
   ''' <param name="aValue">Value to rotate.</param>
   ''' <param name="rotateValue">Number of bits to rotate.</param>
   ''' <returns><paramref name="aValue"/> rotated left by <paramref name="rotateValue"/> bits.</returns>
   Public Shared Function RotateLeft(aValue As Long, rotateValue As Integer) As Long
      Dim normalizedShiftValue As Integer = rotateValue And SHIFT_MASK_FOR_LONG

      Return (aValue << normalizedShiftValue) Or UnsignedShiftRight(aValue, BIT_SIZE_FOR_LONG - normalizedShiftValue)
   End Function

   ''' <summary>
   ''' Rotate a value to the left.
   ''' </summary>
   ''' <param name="aValue">Value to rotate.</param>
   ''' <param name="rotateValue">Number of bits to rotate.</param>
   ''' <returns><paramref name="aValue"/> rotated left by <paramref name="rotateValue"/> bits.</returns>
   Public Shared Function RotateLeft(aValue As ULong, rotateValue As Integer) As ULong
      Dim normalizedShiftValue As Integer = rotateValue And SHIFT_MASK_FOR_LONG

      Return (aValue << normalizedShiftValue) Or (aValue >> BIT_SIZE_FOR_LONG - normalizedShiftValue)
   End Function

   ''' <summary>
   ''' Rotate a value to the left where the number is treated as a <code>Long</code> value.
   ''' </summary>
   ''' <param name="aValue">Value to rotate.</param>
   ''' <param name="rotateValue">Number of bits to rotate.</param>
   ''' <returns><paramref name="aValue"/> rotated left by <paramref name="rotateValue"/> bits as if it were a <c>Long</c> value.</returns>
   Public Shared Function RotateLeftForLong(aValue As BigInteger, rotateValue As Integer) As BigInteger
      Dim normalizedShiftValue As Integer = rotateValue And SHIFT_MASK_FOR_LONG

      Return (aValue << normalizedShiftValue) Or UnsignedShiftRightForLong(aValue, BIT_SIZE_FOR_LONG - normalizedShiftValue)
   End Function
#End Region

#Region "Rotate right methods"
   ''' <summary>
   ''' Rotate a value to the right.
   ''' </summary>
   ''' <param name="aValue">Value to rotate.</param>
   ''' <param name="rotateValue">Number of bits to rotate.</param>
   ''' <returns><paramref name="aValue"/> rotated right by <paramref name="rotateValue"/> bits.</returns>
   Public Shared Function RotateRight(aValue As SByte, rotateValue As Integer) As SByte
      Dim normalizedShiftValue As Integer = rotateValue And SHIFT_MASK_FOR_BYTE

      Return UnsignedShiftRight(aValue, normalizedShiftValue) Or (aValue << (BIT_SIZE_FOR_BYTE - normalizedShiftValue))
   End Function

   ''' <summary>
   ''' Rotate a value to the right.
   ''' </summary>
   ''' <param name="aValue">Value to rotate.</param>
   ''' <param name="rotateValue">Number of bits to rotate.</param>
   ''' <returns><paramref name="aValue"/> rotated right by <paramref name="rotateValue"/> bits.</returns>
   Public Shared Function RotateRight(aValue As Byte, rotateValue As Integer) As Byte
      Dim normalizedShiftValue As Integer = rotateValue And SHIFT_MASK_FOR_BYTE

      Return (aValue >> normalizedShiftValue) Or (aValue << (BIT_SIZE_FOR_BYTE - normalizedShiftValue))
   End Function

   ''' <summary>
   ''' Rotate a value to the right.
   ''' </summary>
   ''' <param name="aValue">Value to rotate.</param>
   ''' <param name="rotateValue">Number of bits to rotate.</param>
   ''' <returns><paramref name="aValue"/> rotated right by <paramref name="rotateValue"/> bits.</returns>
   Public Shared Function RotateRight(aValue As Short, rotateValue As Integer) As Short
      Dim normalizedShiftValue As Integer = rotateValue And SHIFT_MASK_FOR_SHORT

      Return UnsignedShiftRight(aValue, normalizedShiftValue) Or (aValue << (BIT_SIZE_FOR_SHORT - normalizedShiftValue))
   End Function

   ''' <summary>
   ''' Rotate a value to the right.
   ''' </summary>
   ''' <param name="aValue">Value to rotate.</param>
   ''' <param name="rotateValue">Number of bits to rotate.</param>
   ''' <returns><paramref name="aValue"/> rotated right by <paramref name="rotateValue"/> bits.</returns>
   Public Shared Function RotateRight(aValue As UShort, rotateValue As Integer) As UShort
      Dim normalizedShiftValue As Integer = rotateValue And SHIFT_MASK_FOR_SHORT

      Return (aValue >> normalizedShiftValue) Or (aValue << (BIT_SIZE_FOR_SHORT - normalizedShiftValue))
   End Function

   ''' <summary>
   ''' Rotate a value to the right.
   ''' </summary>
   ''' <param name="aValue">Value to rotate.</param>
   ''' <param name="rotateValue">Number of bits to rotate.</param>
   ''' <returns><paramref name="aValue"/> rotated right by <paramref name="rotateValue"/> bits.</returns>
   Public Shared Function RotateRight(aValue As Integer, rotateValue As Integer) As Integer
      Dim normalizedShiftValue As Integer = rotateValue And SHIFT_MASK_FOR_INTEGER

      Return UnsignedShiftRight(aValue, normalizedShiftValue) Or (aValue << (BIT_SIZE_FOR_INTEGER - normalizedShiftValue))
   End Function

   ''' <summary>
   ''' Rotate a value to the right.
   ''' </summary>
   ''' <param name="aValue">Value to rotate.</param>
   ''' <param name="rotateValue">Number of bits to rotate.</param>
   ''' <returns><paramref name="aValue"/> rotated right by <paramref name="rotateValue"/> bits.</returns>
   Public Shared Function RotateRight(aValue As UInteger, rotateValue As Integer) As UInteger
      Dim normalizedShiftValue As Integer = rotateValue And SHIFT_MASK_FOR_INTEGER

      Return (aValue >> normalizedShiftValue) Or (aValue << (BIT_SIZE_FOR_INTEGER - normalizedShiftValue))
   End Function

   ''' <summary>
   ''' Rotate a value to the right.
   ''' </summary>
   ''' <param name="aValue">Value to rotate.</param>
   ''' <param name="rotateValue">Number of bits to rotate.</param>
   ''' <returns><paramref name="aValue"/> rotated right by <paramref name="rotateValue"/> bits.</returns>
   Public Shared Function RotateRight(aValue As Long, rotateValue As Integer) As Long
      Dim normalizedShiftValue As Integer = rotateValue And SHIFT_MASK_FOR_LONG

      Return UnsignedShiftRight(aValue, normalizedShiftValue) Or (aValue << (BIT_SIZE_FOR_LONG - normalizedShiftValue))
   End Function

   ''' <summary>
   ''' Rotate a value to the right.
   ''' </summary>
   ''' <param name="aValue">Value to rotate.</param>
   ''' <param name="rotateValue">Number of bits to rotate.</param>
   ''' <returns><paramref name="aValue"/> rotated right by <paramref name="rotateValue"/> bits.</returns>
   Public Shared Function RotateRight(aValue As ULong, rotateValue As Integer) As ULong
      Dim normalizedShiftValue As Integer = rotateValue And SHIFT_MASK_FOR_LONG

      Return (aValue >> normalizedShiftValue) Or (aValue << (BIT_SIZE_FOR_LONG - normalizedShiftValue))
   End Function

   ''' <summary>
   ''' Rotate a value to the right where the number is treated as a <code>Long</code> value.
   ''' </summary>
   ''' <param name="aValue">Value to rotate.</param>
   ''' <param name="rotateValue">Number of bits to rotate.</param>
   ''' <returns><paramref name="aValue"/> rotated right by <paramref name="rotateValue"/> bits as if it were a <c>Long</c> value.</returns>
   Public Shared Function RotateRightForLong(aValue As BigInteger, rotateValue As Integer) As BigInteger
      Dim normalizedShiftValue As Integer = rotateValue And SHIFT_MASK_FOR_LONG

      Return UnsignedShiftRightForLong(aValue, normalizedShiftValue) Or (aValue << (BIT_SIZE_FOR_LONG - normalizedShiftValue))
   End Function
#End Region
#End Region
#End Region
End Class
