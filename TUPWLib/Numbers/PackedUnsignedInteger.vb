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
'    2020-04-21: V1.0.0: Created.
'

Option Strict On
Option Explicit On

''' <summary>
''' Converts integers from and to an unsigned packed byte array
''' </summary>
Public NotInheritable Class PackedUnsignedInteger
#Region "Private constants"
   '******************************************************************
   ' Private constants
   '******************************************************************

   '
   ' Constants for conversion
   '
   Private Const START_1_BYTE_VALUE As Integer = 0                '       &H00
   Private Const START_2_BYTE_VALUE As Integer = 64               '       &H40
   Private Const START_3_BYTE_VALUE As Integer = 16448            '     &H4040
   Private Const START_4_BYTE_VALUE As Integer = 4210752          '   &H404040
   Private Const START_TOO_LARGE_VALUE As Integer = 1077952576    ' &H40404040

   '
   ' Constants for masks
   '
   Private Const NO_LENGTH_MASK_FOR_BYTE As Byte = &H3F
   Private Const BYTE_MASK_FOR_INTEGER As Integer = &HFF

   '
   ' Constants for length indicators
   '
   '   Private Const LENGTH_1_MASK As Byte = 0
   Private Const LENGTH_2_MASK As Byte = &H40
   Private Const LENGTH_3_MASK As Byte = &H80
   Private Const LENGTH_4_MASK As Byte = &HC0

   '
   ' Constant for calculation
   '

   '
   ' It is not possible to write a byte constant in VB.
   '
   Private Const ONE_AS_BYTE As Byte = 1US
#End Region

#Region "Public methods"
   '******************************************************************
   ' Public methods
   '******************************************************************

   ''' <summary>
   ''' Convert an integer into a packed unsigned integer byte array
   ''' </summary>
   ''' <remarks>
   ''' Valid integers range from 0 to 1,077,952,575.
   ''' All other numbers throw an <see cref="ArgumentException"/>.
   ''' </remarks>
   ''' <param name="anInteger">Number to convert to a packed unsigned integer</param>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="anInteger"/> has not a value between 0 and 1,077,952,575 (inclusive)</exception>
   ''' <returns>packed unsigned integer byte array with integer as value</returns>
   Public Shared Function FromInteger(anInteger As Integer) As Byte()
      Dim result As Byte()
      Dim intermediateInteger As Integer

      Select Case anInteger
         Case < START_1_BYTE_VALUE
            Throw New ArgumentException("Integer must not be negative")

         Case < START_2_BYTE_VALUE
            result = (New Byte(0 To 0) {CByte(anInteger)})

         Case < START_3_BYTE_VALUE
            result = New Byte(0 To 1) {}

            intermediateInteger = anInteger - START_2_BYTE_VALUE

            result(1) = CByte(intermediateInteger And BYTE_MASK_FOR_INTEGER)

            intermediateInteger >>= 8
            result(0) = (LENGTH_2_MASK Or CByte(intermediateInteger))

         Case < START_4_BYTE_VALUE
            result = New Byte(0 To 2) {}

            intermediateInteger = anInteger - START_3_BYTE_VALUE

            result(2) = CByte(intermediateInteger And BYTE_MASK_FOR_INTEGER)

            intermediateInteger >>= 8
            result(1) = CByte(intermediateInteger And BYTE_MASK_FOR_INTEGER)

            intermediateInteger >>= 8
            result(0) = (LENGTH_3_MASK Or CByte(intermediateInteger))

         Case < START_TOO_LARGE_VALUE
            result = New Byte(0 To 3) {}

            intermediateInteger = anInteger - START_4_BYTE_VALUE

            result(3) = CByte(intermediateInteger And BYTE_MASK_FOR_INTEGER)

            intermediateInteger >>= 8
            result(2) = CByte(intermediateInteger And BYTE_MASK_FOR_INTEGER)

            intermediateInteger >>= 8
            result(1) = CByte(intermediateInteger And BYTE_MASK_FOR_INTEGER)

            intermediateInteger >>= 8
            result(0) = (LENGTH_4_MASK Or CByte(intermediateInteger))

         Case Else
            Throw New ArgumentException("Integer too large for packed integer")
      End Select

      Return result
   End Function

   ''' <summary>
   ''' Convert a packed unsigned integer byte array in a possibly larger array to an integer.
   ''' </summary>
   ''' <param name="arrayWithPackedNumber">Array in which the packed unsigned integer byte array resides.</param>
   ''' <param name="startIndex">Start index of packed unsigned integer byte array in the byte array.</param>
   ''' <returns>Converted integer (value between 0 and 1,077,952,575).</returns>
   Public Shared Function ToInteger(arrayWithPackedNumber As Byte(), startIndex As Integer) As Integer
      RequireNonNull(arrayWithPackedNumber, NameOf(arrayWithPackedNumber))

      Dim result As Integer

      Dim expectedLength As Integer = GetExpectedLengthWithoutCheck(arrayWithPackedNumber, startIndex)

      If (startIndex + expectedLength) <= arrayWithPackedNumber.Length Then
         Select Case expectedLength
            Case 1
               result = (arrayWithPackedNumber(startIndex) And NO_LENGTH_MASK_FOR_BYTE)

            Case 2
               result = ((CInt(arrayWithPackedNumber(startIndex) And NO_LENGTH_MASK_FOR_BYTE) << 8) Or
                       arrayWithPackedNumber(startIndex + 1)) +
                     START_2_BYTE_VALUE

            Case 3
               result = ((((CInt(arrayWithPackedNumber(startIndex) And NO_LENGTH_MASK_FOR_BYTE) << 8) Or
                      arrayWithPackedNumber(startIndex + 1)) << 8) Or
                      arrayWithPackedNumber(startIndex + 2)) +
                     START_3_BYTE_VALUE


            Case 4
               result = ((((((CInt(arrayWithPackedNumber(startIndex) And NO_LENGTH_MASK_FOR_BYTE) << 8) Or
                      arrayWithPackedNumber(startIndex + 1)) << 8) Or
                      arrayWithPackedNumber(startIndex + 2)) << 8) Or
                      arrayWithPackedNumber(startIndex + 3)) +
                     START_4_BYTE_VALUE

               ' There is no "else" case as all possible values of "expectedLength" are covered
         End Select
      Else
         Throw New ArgumentException("Array too short for packed unsigned integer")
      End If

      Return result
   End Function

   ''' <summary>
   ''' Convert a packed unsigned integer byte array into an integer.
   ''' </summary>
   ''' <param name="aPackedUnsignedInteger">Packed unsigned integer byte array.</param>
   ''' <exception cref="ArgumentException">Thrown if the actual length of the packed number does not match the expected length.</exception>
   ''' <returns>Converted integer (value between 0 and 1,077,952,575).</returns>
   Public Shared Function ToInteger(aPackedUnsignedInteger As Byte()) As Integer
      Return ToInteger(aPackedUnsignedInteger, 0)
   End Function

   ''' <summary>
   ''' Get expected length of packed unsigned integer byte array in a a possibly larger array.
   ''' </summary>
   ''' <param name="arrayWithPackedNumber">Array in which the packed unsigned integer byte array resides.</param>
   ''' <param name="startIndex">Start index of packed unsigned integer byte array in the byte array.</param>
   ''' <returns>Expected length (1 to 4)</returns>
   Public Shared Function GetExpectedLength(arrayWithPackedNumber As Byte(), startindex As Integer) As Byte
      RequireNonNull(arrayWithPackedNumber, NameOf(arrayWithPackedNumber))

      Return GetExpectedLengthWithoutCheck(arrayWithPackedNumber, startindex)
   End Function

   ''' <summary>
   ''' Get expected length of packed unsigned integer byte array from first byte.
   ''' </summary>
   ''' <param name="aPackedUnsignedInteger">Packed unsigned integer byte array.</param>
   ''' <returns>Expected length (1 to 4)</returns>
   Public Shared Function GetExpectedLength(aPackedUnsignedInteger As Byte()) As Byte
      Return GetExpectedLength(aPackedUnsignedInteger, 0)
   End Function

   ''' <summary>
   ''' Convert a decimal byte array that is supposed to be a packed unsigned integer
   ''' into a string.
   ''' </summary>
   ''' <param name="aPackedUnsignedInteger">Byte array of packed unsigned integer</param>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="aPackedUnsignedInteger"/> is <c>Nothing</c>.</exception>
   ''' <returns>String representation of the given packed unsigned integer</returns>
#Disable Warning BC40005 ' Member shadows an overridable method in the base type: This does *not* override Object.ToString()
   Public Shared Function ToString(aPackedUnsignedInteger As Byte()) As String
#Enable Warning BC40005 ' Member shadows an overridable method in the base type
      Return ToInteger(aPackedUnsignedInteger).ToString()
   End Function
#End Region

#Region "Private methods"

#Region "Internal calculation methods"
   ''' <summary>
   ''' Get expected length of packed unsigned integer byte array in a a possibly larger array.
   ''' </summary>
   ''' <remarks>This method does not check if the supplied array is <c>Nothing</c> as it assumes that this check has already been made.</remarks>
   ''' <param name="arrayWithPackedNumber">Array in which the packed unsigned integer byte array resides.</param>
   ''' <param name="startIndex">Start index of packed unsigned integer byte array in the byte array.</param>
   ''' <returns>Expected length (1 to 4)</returns>
   Private Shared Function GetExpectedLengthWithoutCheck(arrayWithPackedNumber As Byte(), startindex As Integer) As Byte
      Return (arrayWithPackedNumber(startindex) >> 6) + ONE_AS_BYTE
   End Function
#End Region

#Region "Exception helper methods"
   ''' <summary>
   ''' Check if object is null and throw an exception, if it is.
   ''' </summary>
   ''' <param name="anObject">Object to check.</param>
   ''' <param name="parameterName">Parameter name for exception.</param>
   ''' <exception cref="ArgumentNullException">Thrown when <paramref name="anObject"/> is <c>Nothing</c>.</exception>
   Private Shared Sub RequireNonNull(anObject As Object, parameterName As String)
      If anObject Is Nothing Then _
         Throw New ArgumentNullException(parameterName)
   End Sub
#End Region
#End Region
End Class
