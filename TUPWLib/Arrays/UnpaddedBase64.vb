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
'    2020-05-07: V1.0.0: Created.
'    2020-05-18: V1.0.1: Added null check.
'

Option Strict On
Option Explicit On

''' <summary>
''' Wrapper for unpadded Base64 conversions.
''' </summary>
Public NotInheritable Class UnpaddedBase64
#Region "Private constants"
   Private Const PADDING_CHARACTER As Char = "="c
#End Region

#Region "Public methods"
   ''' <summary>
   ''' Convert unpadded Base64 string to byte array.
   ''' </summary>
   ''' <param name="aBase64String">Base64 encoded string to convert.</param>
   ''' <returns>Byte array encoded as Base64 string.</returns>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="aBase64String"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="FormatException">Thrown if an invalid Base64 string used.</exception>
   Public Shared Function FromUnpaddedBase64String(aBase64String As String) As Byte()
      If aBase64String Is Nothing Then _
         Throw New ArgumentNullException(NameOf(aBase64String))

      Dim result As Byte()

      Dim padLength As Integer = 4 - (aBase64String.Length And &H3)

      If padLength = 4 Then _
         padLength = 0

      If padLength > 0 Then
         If aBase64String(aBase64String.Length - 1) <> PADDING_CHARACTER Then
            Dim repaddedString As String = aBase64String.PadRight(aBase64String.Length + padLength, PADDING_CHARACTER)

            result = Convert.FromBase64String(repaddedString)
         Else
            ' The Base64 string ends with a padding character and has the correct padded Base64 string length.
            ' We assume that it is a padded Base64 string and try to decode it normally.
            result = Convert.FromBase64String(aBase64String)
         End If
      Else
         result = Convert.FromBase64String(aBase64String)
      End If

      Return result
   End Function

   ''' <summary>
   ''' Convert a byte array to an unpadded Base64 string.
   ''' </summary>
   ''' <param name="aByteArray">Byte array to convert.</param>
   ''' <returns>Unpadded Base64 encoding of the supplied byte array.</returns>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="aByteArray"/> is <c>Nothing</c>.</exception>
   Public Shared Function ToUnpaddedBase64String(aByteArray As Byte()) As String
      If aByteArray Is Nothing Then _
         Throw New ArgumentNullException(NameOf(aByteArray))

      Dim result As String = Convert.ToBase64String(aByteArray)

      Return result.Substring(0, IndexOfLastNonTerminator(result, PADDING_CHARACTER) + 1)  ' + 1 as we need the length, not the position
   End Function
#End Region

#Region "Private methods"
   ''' <summary>
   ''' Get index of the last character that is not the supplied terminator character.
   ''' </summary>
   ''' <param name="aString">String to search in.</param>
   ''' <param name="terminator">Character that should not be at the end.</param>
   ''' <returns>Index of last character that is not the supplied <paramref name="terminator"/>, 
   ''' or <c>-1</c> if the string is empty or consist only of <paramref name="terminator"/> characters.</returns>
   Private Shared Function IndexOfLastNonTerminator(aString As String, terminator As Char) As Integer
      Dim result As Integer = -1

      For i As Integer = aString.Length - 1 To 0 Step -1
         If aString(i) <> terminator Then
            result = i
            Exit For
         End If
      Next

      Return result
   End Function
#End Region
End Class
