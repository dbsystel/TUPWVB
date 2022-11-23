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
' Version: 1.0.0
'
' Change history:
'    2020-05-06: V1.0.0: Created.
'

Option Strict On
Option Explicit On

Imports System.Numerics

''' <summary>
''' Safely convert integer values between signed and unsigned types without overflows while retaining the binary values.
''' </summary>
''' <remarks>E.g., an SByte value of -17 (&amp;HEF) is converted to a byte value of 239 (&amp;HEF) and vice versa.</remarks>
Public NotInheritable Class BitwiseIntegerConverter
#Region "Private constants"
   '
   ' These constants are declared here as it is not possible to define a byte constant in VB.
   ' One can say "&H8000US" to specify an unsigned short constant, but there is no such thing
   ' as "&H80UB" to specify an unsigned byte constant. If one uses just "&H80" in the code
   ' this really means "&H00000080I" and leads to conversions to integer for the calculation
   ' which makes them inefficient.
   '
   Private Const NO_SIGN_FOR_UNSIGNED_BYTE_MASK As Byte = &H7FUS
   Private Const SIGN_FOR_UNSIGNED_BYTE_MASK As Byte = Not NO_SIGN_FOR_UNSIGNED_BYTE_MASK
   Private Const NO_SIGN_FOR_SIGNED_BYTE_MASK As SByte = &H7FS
   Private Const SIGN_FOR_SIGNED_BYTE_MASK As SByte = Not NO_SIGN_FOR_SIGNED_BYTE_MASK ' The compiler complains when "&H80S" is used here

   '
   ' Constants for BigInteger to Long conversion
   '
   Private Const UNSIGNED_LONG_MASK As Long = &H7FFFFFFFFFFFFFFFL
   Private Const ONLY_SIGN_LONG_MASK As Long = &H8000000000000000L
   Private Const ONLY_SIGN_ULONG_MASK As ULong = &H8000000000000000UL
#End Region

#Region "Elementary data type conversion methods"
   ''' <summary>
   ''' Converts a signed byte to an unsigned byte.
   ''' </summary>
   ''' <param name="aValue">Signed byte value to convert.</param>
   ''' <returns>Unsigned byte value with the same binary value.</returns>
   Public Shared Function AsUnsignedByte(aValue As SByte) As Byte
      Dim result As Byte

      If (aValue And &H80) <> 0 Then
         result = CByte(aValue And NO_SIGN_FOR_SIGNED_BYTE_MASK)
         result = result Or SIGN_FOR_UNSIGNED_BYTE_MASK
      Else
         result = CByte(aValue)
      End If

      Return result
   End Function

   ''' <summary>
   ''' Converts a signed short to an unsigned byte.
   ''' </summary>
   ''' <param name="aValue">Signed short value to convert.</param>
   ''' <returns>Unsigned byte value with the same binary value as the least significant byte as <paramref name="aValue"/>.</returns>
   Public Shared Function AsUnsignedByte(aValue As Short) As Byte
      Return AsUnsignedByte(CSByte(aValue And &HFFS))
   End Function

   ''' <summary>
   ''' Converts an unsigned byte to a signed byte.
   ''' </summary>
   ''' <param name="aValue">Unsigned byte value to convert.</param>
   ''' <returns>Signed byte value with the same binary value.</returns>
   Public Shared Function AsSignedByte(aValue As Byte) As SByte
      Dim result As SByte

      If (aValue And &H80) <> 0 Then
         result = CSByte(aValue And NO_SIGN_FOR_UNSIGNED_BYTE_MASK)
         result = result Or SIGN_FOR_SIGNED_BYTE_MASK
      Else
         result = CSByte(aValue)
      End If

      Return result
   End Function

   ''' <summary>
   ''' Converts an unsigned short to an signed byte.
   ''' </summary>
   ''' <param name="aValue">Unsigned short value to convert.</param>
   ''' <returns>Signed byte value with the same binary value as the least significant byte as <paramref name="aValue"/>.</returns>
   Public Shared Function AsSignedByte(aValue As UShort) As SByte
      Return AsSignedByte(CByte(aValue And &HFFUS))
   End Function

   ''' <summary>
   ''' Converts a signed short to an unsigned short.
   ''' </summary>
   ''' <param name="aValue">Signed short value to convert.</param>
   ''' <returns>Unsigned short value with the same binary value.</returns>
   Public Shared Function AsUnsignedShort(aValue As Short) As UShort
      Dim result As UShort

      If (aValue And &H8000S) <> 0S Then
         result = CUShort(aValue And &H7FFFS)
         result = result Or &H8000US
      Else
         result = CUShort(aValue)
      End If

      Return result
   End Function

   ''' <summary>
   ''' Converts a signed integer to an unsigned short.
   ''' </summary>
   ''' <param name="aValue">Signed integer value to convert.</param>
   ''' <returns>Unsigned short value with the same binary value as the least significant word as <paramref name="aValue"/>.</returns>
   Public Shared Function AsUnsignedShort(aValue As Integer) As UShort
      Return AsUnsignedShort(CShort(aValue And &HFFFFI))
   End Function

   ''' <summary>
   ''' Converts an unsigned short to a signed short.
   ''' </summary>
   ''' <param name="aValue">Unsigned short value to convert.</param>
   ''' <returns>Signed short value with the same binary value.</returns>
   Public Shared Function AsSignedShort(aValue As UShort) As Short
      Dim result As Short

      If (aValue And &H8000US) <> 0US Then
         result = CShort(aValue And &H7FFFUS)
         result = result Or &H8000S
      Else
         result = CShort(aValue)
      End If

      Return result
   End Function

   ''' <summary>
   ''' Converts an unsigned integer to a short.
   ''' </summary>
   ''' <param name="aValue">Unsigned integer value to convert.</param>
   ''' <returns>Signed short value with the same binary value as the least significant word as <paramref name="aValue"/>.</returns>
   Public Shared Function AsSignedShort(aValue As UInteger) As Short
      Return AsSignedShort(CUShort(aValue And &HFFFFUI))
   End Function

   ''' <summary>
   ''' Converts a signed integer to an unsigned integer.
   ''' </summary>
   ''' <param name="aValue">Signed integer value to convert.</param>
   ''' <returns>Unsigned integer value with the same binary value.</returns>
   Public Shared Function AsUnsignedInteger(aValue As Integer) As UInteger
      Dim result As UInteger

      If (aValue And &H80000000I) <> 0I Then
         result = CUInt(aValue And &H7FFFFFFFI)
         result = result Or &H80000000UI
      Else
         result = CUInt(aValue)
      End If

      Return result
   End Function

   ''' <summary>
   ''' Converts a signed long to an unsigned integer.
   ''' </summary>
   ''' <param name="aValue">Signed long value to convert.</param>
   ''' <returns>Unsigned integer value with the same binary value as the least significant 4 bytes as <paramref name="aValue"/>.</returns>
   Public Shared Function AsUnsignedInteger(aValue As Long) As UInteger
      Return AsUnsignedInteger(CInt(aValue And &HFFFFFFFFL))
   End Function

   ''' <summary>
   ''' Converts an unsigned integer to a signed integer.
   ''' </summary>
   ''' <param name="aValue">Unsigned integer value to convert.</param>
   ''' <returns>Signed integer value with the same binary value.</returns>
   Public Shared Function AsSignedInteger(aValue As UInteger) As Integer
      Dim result As Integer

      If (aValue And &H80000000UI) <> 0UI Then
         result = CInt(aValue And &H7FFFFFFFUI)
         result = result Or &H80000000I
      Else
         result = CInt(aValue)
      End If

      Return result
   End Function

   ''' <summary>
   ''' Converts an unsigned long to an signed integer.
   ''' </summary>
   ''' <param name="aValue">Unigned long value to convert.</param>
   ''' <returns>Signed integer value with the same binary value as the least significant 4 bytes as <paramref name="aValue"/>.</returns>
   Public Shared Function AsSignedInteger(aValue As ULong) As Integer
      Return AsSignedInteger(CUInt(aValue And &HFFFFFFFFUL))
   End Function

   ''' <summary>
   ''' Converts a signed long to an unsigned long.
   ''' </summary>
   ''' <param name="aValue">Signed long value to convert.</param>
   ''' <returns>Unsigned long value with the same binary value.</returns>
   Public Shared Function AsUnsignedLong(aValue As Long) As ULong
      Dim result As ULong

      If (aValue And &H8000000000000000L) <> 0L Then
         result = CULng(aValue And &H7FFFFFFFFFFFFFFFL)
         result = result Or &H8000000000000000UL
      Else
         result = CULng(aValue)
      End If

      Return result
   End Function

   ''' <summary>
   ''' Converts an unsigned long to a signed long.
   ''' </summary>
   ''' <param name="aValue">Unsigned long  value to convert.</param>
   ''' <returns>Signed long value with the same binary value.</returns>
   Public Shared Function AsSignedLong(aValue As ULong) As Long
      Dim result As Long

      If (aValue And &H8000000000000000UL) <> 0UL Then
         result = CLng(aValue And &H7FFFFFFFFFFFFFFFUL)
         result = result Or &H8000000000000000L
      Else
         result = CLng(aValue)
      End If

      Return result
   End Function
#End Region

#Region "BigInteger conversion methods"
   Public Shared Function GetLowLongOfBigInteger(bi As BigInteger) As BigInteger
      Dim result As BigInteger

      '
      ' BigInteger always sign-extends a number, so it is not possible to mask
      ' with &HFFFFFFFFFFFFFFFFL. This will return the BigInteger unchanged.
      ' So we need to mask with an unsigned mask and add the sign bit later
      ' again if there was one.
      '
      Dim hasSignBit As Boolean = (bi And ONLY_SIGN_ULONG_MASK) <> 0

      result = bi And UNSIGNED_LONG_MASK  ' This is not sign-extended and will mask out the number

      If hasSignBit Then _
         result = result Or ONLY_SIGN_LONG_MASK   ' We need to add the sign bit again, if there was one

      Return result
   End Function
#End Region
End Class
