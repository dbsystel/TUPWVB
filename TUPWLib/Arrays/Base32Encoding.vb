'
' SPDX-FileCopyrightText: 2021 DB Systel GmbH
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
' Version: 1.3.0
'
' Change history:
'    2020-11-12: V1.0.0: Created.
'    2020-11-13: V1.1.0: Use unified method for mapping tables.
'    2021-01-04: V1.1.1: Corrected typo.
'    2021-05-12: V1.2.0: Throw correct exception on invalid character.
'    2021-05-12: V1.3.0: Direct mapping of a byte value to a char.
'

''' <summary>
''' Converts byte arrays from and to Base32 encoding either, as specified in RFC4868, or in spell-safe format.
''' </summary>
Public NotInheritable Class Base32Encoding
#Region "Private constants"
#Region "Error messages"
   Private Const ERROR_TEXT_INVALID_CHARACTER As String = "Character is not a valid Base32 character"
   Private Const ERROR_TEXT_INVALID_STRING_LENGTH As String = "Invalid Base32 string length"
   Private Const ERROR_DESTINATION_TOO_SMALL As String = "Buffer too small"
#End Region

#Region "Processing constants"
   Private Const BITS_PER_CHARACTER As Byte = 5
   Private Const BITS_PER_BYTE As Byte = 8
   Private Const BITS_DIFFERENCE As Byte = BITS_PER_BYTE - BITS_PER_CHARACTER
   Private Const CHARACTER_MASK As Byte = 31
   Private Const INVALID_CHARACTER_VALUE As Byte = 255
   Private Const PADDING_CHARACTER As Char = "="c
   Private Const CODEPOINT_ZERO As Integer = 48
#End Region

#Region "Mapping tables"
   ' Mapping tables

#Region "RFC4648"
   ' RFC 4648

   '
   ' This Is the RFC 4648 mapping:
   '
   ' Value      0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31
   ' Character  A  B  C  D  E  F  G  H  I  J  K  L  M  N  O  P  Q  R  S  T  U  V  W  X  Y  Z  2  3  4  5  6  7
   '

   ''' <summary>
   ''' Mapping from a byte value to an RFC 4648 Base32 character
   ''' </summary>
   Private Shared ReadOnly RFC_4648_VALUE_TO_CHAR As Char() = {"A"c, "B"c, "C"c, "D"c, "E"c, "F"c, "G"c, "H"c,
            "I"c, "J"c, "K"c, "L"c, "M"c, "N"c, "O"c, "P"c,
            "Q"c, "R"c, "S"c, "T"c, "U"c, "V"c, "W"c, "X"c,
            "Y"c, "Z"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c}

   ''' <summary>
   ''' Mapping from an RFC 4648 Base32 character to byte value ('0'-based)
   ''' </summary>
   Private Shared ReadOnly RFC_4648_CHAR_TO_VALUE As Byte() = {255, 255, 26, 27, 28, 29, 30, 31,
            255, 255, 255, 255, 255, 255, 255, 255,
            255, 0, 1, 2, 3, 4, 5, 6,
            7, 8, 9, 10, 11, 12, 13, 14,
            15, 16, 17, 18, 19, 20, 21, 22,
            23, 24, 25, 255, 255, 255, 255, 255,
            255, 0, 1, 2, 3, 4, 5, 6,
            7, 8, 9, 10, 11, 12, 13, 14,
            15, 16, 17, 18, 19, 20, 21, 22,
            23, 24, 25}
#End Region

#Region "Spell-safe"
   ' Spell-safe

   '
   ' This Is the spell-safe mapping:
   '
   ' Value      0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31
   ' Character  2  3  4  5  6  7  8  9  C  D  G  H  J  K  N  P  T  V  X  Z  c  d  g  h  j  k  n  p  t  v  x  z
   '
   ' The mapping is constructed so that there are no vowels, no B that can be confused with 8,
   ' no S that can be confused with 5, no O Or Q that can be confused with 0,
   ' no 1, I And L that can be confused with each other, no R that can be confused with P And
   ' no U And W that can be confused with each other and with V.
   '

   ''' <summary>
   ''' Mapping from a byte value to a spell-safe Base32 character
   ''' </summary>
   Private Shared ReadOnly SPELL_SAFE_VALUE_TO_CHAR As Char() = {"2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c,
            "C"c, "D"c, "G"c, "H"c, "J"c, "K"c, "N"c, "P"c,
            "T"c, "V"c, "X"c, "Z"c, "c"c, "d"c, "g"c, "h"c,
            "j"c, "k"c, "n"c, "p"c, "t"c, "v"c, "x"c, "z"c}

   ''' <summary>
   ''' Mapping from a spell-safe Base32 character to byte value ('0'-based)
   ''' </summary>
   Private Shared ReadOnly SPELL_SAFE_CHAR_TO_VALUE As Byte() = {255, 255, 0, 1, 2, 3, 4, 5,
            6, 7, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 8, 9, 255, 255, 10,
            11, 255, 12, 13, 255, 255, 14, 255,
            15, 255, 255, 255, 16, 255, 17, 255,
            18, 255, 19, 255, 255, 255, 255, 255,
            255, 255, 255, 20, 21, 255, 255, 22,
            23, 255, 24, 25, 255, 255, 26, 255,
            27, 255, 255, 255, 28, 255, 29, 255,
            30, 255, 31}
#End Region
#End Region
#End Region

#Region "Public methods"
#Region "Decode methods"
   ''' <summary>
   ''' Decodes a Base32 string into a new byte array.
   ''' </summary>
   ''' <param name="encodedValue">The Base32 string to decode.</param>
   ''' <returns>The decoded Base32 string as a byte array.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="encodedValue"/> has an invalid length.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="encodedValue"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="FormatException">Thrown if <paramref name="encodedValue"/> contains an invalid character.</exception>
   Public Shared Function Decode(encodedValue As String) As Byte()
      Return DecodeNewBufferWithMapping(encodedValue, RFC_4648_CHAR_TO_VALUE)
   End Function

   ''' <summary>
   ''' Decodes a Base32 string into an existing byte array.
   ''' </summary>
   ''' <param name="encodedValue">The Base32 string to decode.</param>
   ''' <param name="destinationBuffer">Byte array where the decoded values are placed.</param>
   ''' <returns>The length of the bytes written into the destination buffer.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="encodedValue"/> has an invalid length.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="encodedValue"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="FormatException">Thrown if <paramref name="encodedValue"/> contains an invalid character.</exception>
   Public Shared Function Decode(encodedValue As String, destinationBuffer As Byte()) As Integer
      Return DecodeExistingBufferWithMapping(encodedValue, destinationBuffer, RFC_4648_CHAR_TO_VALUE)   ' destinationBuffer is checked in the called method
   End Function

   ''' <summary>
   ''' Decodes a spell-safe Base32 string into a byte array.
   ''' </summary>
   ''' <param name="encodedValue">The Base32 string to decode.</param>
   ''' <returns>The decoded spell-safe Base32 string as a byte array.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="encodedValue"/> contains an invalid spell-safe Base32 character, or has an invalid length.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="encodedValue"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="FormatException">Thrown if <paramref name="encodedValue"/> contains an invalid character.</exception>
   Public Shared Function DecodeSpellSafe(encodedValue As String) As Byte()
      Return DecodeNewBufferWithMapping(encodedValue, SPELL_SAFE_CHAR_TO_VALUE)
   End Function

   ''' <summary>
   ''' Decodes a spell-safe Base32 string into an existing byte array.
   ''' </summary>
   ''' <param name="encodedValue">The Base32 string to decode.</param>
   ''' <param name="destinationBuffer">Byte array where the decoded values are placed.</param>
   ''' <returns>The length of the bytes written into the destination buffer.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="encodedValue"/> contains an invalid Base32 character, or has an invalid length.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="encodedValue"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="FormatException">Thrown if <paramref name="encodedValue"/> contains an invalid character.</exception>
   Public Shared Function DecodeSpellSafe(encodedValue As String, destinationBuffer As Byte()) As Integer
      Return DecodeExistingBufferWithMapping(encodedValue, destinationBuffer, SPELL_SAFE_CHAR_TO_VALUE)   ' destinationBuffer is checked in the called method
   End Function
#End Region

#Region "Encode methods"
   ''' <summary>
   ''' Encodes a byte array into a padded Base32 string.
   ''' </summary>
   ''' <param name="aByteArray">The byte array to encode.</param>
   ''' <returns>The Base32 representation of the bytes in <paramref name="aByteArray"/>.</returns>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="aByteArray"/> is <c>Nothing</c>.</exception>
   Public Shared Function Encode(aByteArray As Byte()) As String
      Return EncodeWorker(aByteArray, RFC_4648_VALUE_TO_CHAR, True)
   End Function

   ''' <summary>
   ''' Encodes a byte array into an unpadded Base32 string.
   ''' </summary>
   ''' <param name="aByteArray">The byte array to encode.</param>
   ''' <returns>The Base32 representation of the bytes in <paramref name="aByteArray"/>.</returns>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="aByteArray"/> is <c>Nothing</c>.</exception>
   Public Shared Function EncodeNoPadding(aByteArray As Byte()) As String
      Return EncodeWorker(aByteArray, RFC_4648_VALUE_TO_CHAR, False)
   End Function

   ''' <summary>
   ''' Encodes a byte array into a padded spell-safe Base32 string.
   ''' </summary>
   ''' <param name="aByteArray">The byte array to encode.</param>
   ''' <returns>The spell-safe Base32 representation of the bytes in <paramref name="aByteArray"/>.</returns>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="aByteArray"/> is <c>Nothing</c>.</exception>
   Public Shared Function EncodeSpellSafe(aByteArray As Byte()) As String
      Return EncodeWorker(aByteArray, SPELL_SAFE_VALUE_TO_CHAR, True)
   End Function

   ''' <summary>
   ''' Encodes a byte array into an unpadded spell-safe Base32 string.
   ''' </summary>
   ''' <param name="aByteArray">The byte array to encode.</param>
   ''' <returns>The spell-safe Base32 representation of the bytes in <paramref name="aByteArray"/>.</returns>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="aByteArray"/> is <c>Nothing</c>.</exception>
   Public Shared Function EncodeSpellSafeNoPadding(aByteArray As Byte()) As String
      Return EncodeWorker(aByteArray, SPELL_SAFE_VALUE_TO_CHAR, False)
   End Function
#End Region
#End Region

#Region "Private methods"
#Region "Internal encode and decode methods"
#Region "Decode methods"
   ''' <summary>
   ''' Decode an encoded value to a New byte array with a specified mapping
   ''' </summary>
   ''' <param name="encodedValue">Encoded value to decode.</param>
   ''' <param name="mapCharToByte">Mapping table to use.</param>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="encodedValue"/> has an invalid length.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="encodedValue"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="FormatException">Thrown if <paramref name="encodedValue"/> contains an invalid character.</exception>
   ''' <returns>Newly created byte array with the decoded bytes.</returns>
   Private Shared Function DecodeNewBufferWithMapping(encodedValue As String, mapCharToByte As Byte()) As Byte()
      Dim byteCount As Integer = CheckEncodedValue(encodedValue)

      Dim result As Byte() = New Byte(0 To byteCount - 1) {}

      If byteCount > 0 Then _
         DecodeWorker(encodedValue, result, byteCount, mapCharToByte)

      Return result
   End Function

   ''' <summary>
   ''' Decodes an encoded value to an existing byte array with a specified mapping.
   ''' </summary>
   ''' <param name="encodedValue">Encoded value to decode.</param>
   ''' <param name="destinationBuffer">Byte array where the decoded values are placed.</param>
   ''' <param name="mapCharToByte">Mapping table to use.</param>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="encodedValue"/> has an invalid length.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="encodedValue"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="FormatException">Thrown if <paramref name="encodedValue"/> contains an invalid character.</exception>
   ''' <returns>Number of bytes in the <paramref name="destinationBuffer"/> that are filled.</returns>
   Private Shared Function DecodeExistingBufferWithMapping(encodedValue As String, destinationBuffer As Byte(), mapCharToByte As Byte()) As Integer
      Dim byteCount As Integer = CheckEncodedValue(encodedValue)

      If byteCount <= destinationBuffer.Length Then
         If byteCount > 0 Then _
            DecodeWorker(encodedValue, destinationBuffer, byteCount, mapCharToByte)

         Return byteCount
      Else
         Throw New ArgumentException(ERROR_DESTINATION_TOO_SMALL, NameOf(destinationBuffer))
      End If
   End Function

   ''' <summary>
   ''' Decodes a Base32 string into a byte array.
   ''' </summary>
   ''' <param name="encodedValue">The Base32 string to decode.</param>
   ''' <param name="mapCharToByte">Array with mappings from the character to the corresponding byte.</param>
   ''' <exception cref="FormatException">Thrown if <paramref name="encodedValue"/> contains an invalid character.</exception>
   Private Shared Sub DecodeWorker(encodedValue As String, destinationBuffer As Byte(), byteCount As Integer, mapCharToByte As Byte())
      Dim actByte As Byte = 0
      Dim bitsRemaining As Byte = BITS_PER_BYTE
      Dim mask As Byte
      Dim arrayIndex As Integer = 0

      For i As Integer = 0 To encodedValue.Length - 1
         Dim encodedChar = encodedValue(i)

         If encodedChar = PADDING_CHARACTER Then _
            Exit For

         Dim charValue As Byte = CharToValue(encodedChar, mapCharToByte)

         If (bitsRemaining > BITS_PER_CHARACTER) Then
            mask = charValue << (bitsRemaining - BITS_PER_CHARACTER)
            ' This is *not* a silly bit operation
            ' SonarLint is silly in that it does not consider that this is done in a loop
#Disable Warning S2437 ' Silly bit operations should not be performed
            actByte = actByte Or mask
#Enable Warning S2437 ' Silly bit operations should not be performed
            bitsRemaining -= BITS_PER_CHARACTER
         Else
            mask = charValue >> (BITS_PER_CHARACTER - bitsRemaining)
            actByte = actByte Or mask
            destinationBuffer(arrayIndex) = actByte
            arrayIndex += 1

            bitsRemaining += BITS_DIFFERENCE

            If bitsRemaining < BITS_PER_BYTE Then
               actByte = charValue << bitsRemaining
            Else
               actByte = 0
            End If
         End If
      Next

      ' If we did not end with a full byte, write the remainder
      If arrayIndex < byteCount Then _
         destinationBuffer(arrayIndex) = actByte
   End Sub
#End Region

#Region "Encode methods"
   ''' <summary>
   ''' Encodes a byte array into a Base32 string.
   ''' </summary>
   ''' <param name="aByteArray">The byte array to encode.</param>
   ''' <param name="mapByteToChar">Array with mappings from the byte to the corresponding character.</param>
   ''' <param name="withPadding"><c>True</c>: Result will be padded, <c>False</c>: Result will not be padded</param>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="aByteArray"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="InvalidOperationException">Thrown when there is a bug in the processing of the bytes.</exception>
   ''' <returns>The Base32 representation of the bytes in <paramref name="aByteArray"/>.</returns>
   Private Shared Function EncodeWorker(aByteArray As Byte(), mapByteToChar As Char(), withPadding As Boolean) As String
      Dim lastIndex As Integer
      Dim resultArray As Char() = EncodeInternal(aByteArray, lastIndex, mapByteToChar)

      If withPadding Then _
         lastIndex = PadCharArray(resultArray, lastIndex)

      Dim result As New String(resultArray, 0, lastIndex)

      Array.Clear(resultArray, 0, resultArray.Length)

      Return result
   End Function

   ''' <summary>
   ''' Encodes a byte array into Base32.
   ''' </summary>
   ''' <param name="aByteArray">The byte array to encode.</param>
   ''' <param name="lastIndex">The last index to use. This is a return value!</param>
   ''' <param name="mapByteToChar">Array with mappings from the byte to the corresponding character.</param>
   ''' <returns>The encoded bytes as a string. Note that <paramref name="lastIndex"/> is also a return parameter.</returns>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="aByteArray"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="IndexOutOfRangeException">Thrown when there is a bug in the processing of the bytes.</exception>
   Private Shared Function EncodeInternal(aByteArray As Byte(), ByRef lastIndex As Integer, mapByteToChar As Char()) As Char()
      If aByteArray Is Nothing Then _
         Throw New ArgumentNullException(NameOf(aByteArray))

      If aByteArray.Length > 0 Then
         Dim charCount As Integer = CInt(Math.Ceiling(aByteArray.Length / BITS_PER_CHARACTER)) * BITS_PER_BYTE
         Dim result As Char() = New Char(0 To charCount - 1) {}

         Dim actValue As Byte = 0
         Dim bitsRemaining As Byte = BITS_PER_CHARACTER
         Dim arrayIndex As Integer = 0

         For Each b As Byte In aByteArray
            ' This is *not* a silly bit operation
            ' SonarLint is silly in that it does not consider that this is done in a loop
#Disable Warning S2437 ' Silly bit operations should not be performed
            actValue = actValue Or (b >> (BITS_PER_BYTE - bitsRemaining))
#Enable Warning S2437 ' Silly bit operations should not be performed
            result(arrayIndex) = mapByteToChar(actValue)
            arrayIndex += 1

            If bitsRemaining <= BITS_DIFFERENCE Then
               actValue = (b >> (BITS_DIFFERENCE - bitsRemaining)) And CHARACTER_MASK
               result(arrayIndex) = mapByteToChar(actValue)
               arrayIndex += 1
               bitsRemaining += BITS_PER_CHARACTER
            End If

            bitsRemaining -= BITS_DIFFERENCE
            actValue = (b << bitsRemaining) And CHARACTER_MASK
         Next

         ' If we did not end with a full char
         If arrayIndex < charCount Then
            result(arrayIndex) = mapByteToChar(actValue)
            arrayIndex += 1
         End If

         lastIndex = arrayIndex

         Return result
      Else
         Return Array.Empty(Of Char)
      End If
   End Function
#End Region
#End Region

#Region "Character value conversion methods"
   ''' <summary>
   ''' Maps a character to the corresponding value.
   ''' </summary>
   ''' <param name="c">Character to convert.</param>
   ''' <param name="mapCharToByte">Map table for conversion.</param>
   ''' <exception cref="FormatException">Thrown when the character <paramref name="c"/> is not a valid character for the mapping <paramref name="mapCharToByte"/>.</exception>
   ''' <returns>Value corresponding to character <paramref name="c"/>.</returns>
   Private Shared Function CharToValue(c As Char, mapCharToByte As Byte()) As Byte
      Dim index As Integer = Asc(c) - CODEPOINT_ZERO

      If (index >= 0) AndAlso (index < mapCharToByte.Length) Then
         Dim result As Byte = mapCharToByte(index)

         If result <> INVALID_CHARACTER_VALUE Then
            Return result
         Else
            Throw New FormatException(ERROR_TEXT_INVALID_CHARACTER)
         End If
      Else
         Throw New FormatException(ERROR_TEXT_INVALID_CHARACTER)
      End If
   End Function
#End Region

#Region "String length helper methods"
   ''' <summary>
   ''' Checks if <paramref name="encodedValue"/> has a valid length and returns it, if it has one.
   ''' </summary>
   ''' <param name="encodedValue">The encoded value to check.</param>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="encodedValue"/> has an invalid length.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="encodedValue"/> is <c>Nothing</c>.</exception>
   ''' <returns>The number of decoded bytes in the encoded value.</returns>
   Private Shared Function CheckEncodedValue(encodedValue As String) As Integer
      If encodedValue Is Nothing Then _
         Throw New ArgumentNullException(NameOf(encodedValue))

      Dim lengthWithoutPadding As Integer = LengthWithoutTrailingChar(encodedValue, PADDING_CHARACTER)

      If lengthWithoutPadding > 0 Then
         If IsLengthValid(encodedValue.Length(), lengthWithoutPadding) Then
            Return (lengthWithoutPadding * BITS_PER_CHARACTER) \ BITS_PER_BYTE
         Else
            Throw New ArgumentException(ERROR_TEXT_INVALID_STRING_LENGTH)
         End If
      Else
         Return 0
      End If
   End Function

   ''' <summary>
   ''' Gets length of string without counting a trailing character.
   ''' </summary>
   ''' <param name="sourceString">String to get the length for.</param>
   ''' <param name="trailingChar">Trailing character to ignore.</param>
   ''' <returns>Length of <paramref name="sourceString"/> without counting <paramref name="trailingChar"/> at the end.</returns>
   Private Shared Function LengthWithoutTrailingChar(sourceString As String, trailingChar As Char) As Integer
      For i As Integer = sourceString.Length - 1 To 0 Step -1
         If sourceString(i) <> trailingChar Then _
            Return i + 1
      Next

      ' Only padding characters found or length is 0
      Return 0
   End Function

   ''' <summary>
   ''' Tests if data length is valid for a Base32 string.
   ''' </summary>
   ''' <param name="dataLength">Total length of Base32 string.</param>
   ''' <param name="lengthWithoutPadding">Length of data without padding in Base32 string.</param>
   ''' <returns><c>True</c>: Length is valid, <c>False</c>: Length is invalid.</returns>
   Private Shared Function IsLengthValid(dataLength As Integer, lengthWithoutPadding As Integer) As Boolean
      Dim lastLength As Integer = lengthWithoutPadding Mod BITS_PER_BYTE

      ' 1/3/6 are invalid lengths of the last 8 character block
      If (lastLength = 1) OrElse (lastLength = 3) OrElse (lastLength = 6) Then
         Return False
      Else
         ' If there is padding the length must be divisible by 8
         If dataLength <> lengthWithoutPadding Then
            Return (dataLength Mod BITS_PER_BYTE) = 0
         Else
            Return True
         End If
      End If
   End Function
#End Region

#Region "Character array helper functions"
   ''' <summary>
   ''' Pads a character array to the full length.
   ''' </summary>
   ''' <param name="resultArray">Array to pad.</param>
   ''' <param name="lastIndex">Last index that should e used.</param>
   ''' <returns>Length of padded array.</returns>
   Private Shared Function PadCharArray(resultArray As Char(), lastIndex As Integer) As Integer
      Dim arrayIndex As Integer = lastIndex
      Dim arrayLength As Integer = resultArray.Length

      Do While (arrayIndex < arrayLength)
         resultArray(arrayIndex) = PADDING_CHARACTER
         arrayIndex += 1
      Loop

      Return arrayIndex
   End Function
#End Region
#End Region
End Class
