﻿'
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
' Version: 1.0.1
'
' Change history:
'    2020-04-27: V1.0.0: Created.
'    2020-10-23: V1.0.1: Removed stray comment.
'

''' <summary>
''' Blinding for byte arrays.
''' </summary>
Public NotInheritable Class ByteArrayBlinding
#Region "Private constants"
   '******************************************************************
   ' Private constants
   '******************************************************************

#Region "Constants for exceptions"
   '
   ' Constants for exceptions
   '
   Private Const ERROR_MESSAGE_INVALID_ARRAY As String = "Invalid blinded byte array"
   Private Const ERROR_MESSAGE_INVALID_MIN_LENGTH As String = "Invalid minimum length"

#End Region

#Region "Constants for indexing and lengths"
   '
   ' Constants for indexing and lengths
   '

   '
   ' These are the indices of the lengths array returned by the GetBalancedBlindingLengths method
   '
   Private Const INDEX_LENGTHS_PREFIX_LENGTH As Integer = 0
   Private Const INDEX_LENGTHS_POSTFIX_LENGTH As Integer = 1
   Private Const LENGTHS_LENGTH As Integer = 2

   '
   ' These are the indices of the lengths in the source byte array.
   ' They have the same value as the previous indices but are logically different.
   '
   Private Const INDEX_SOURCE_PREFIX_LENGTH As Integer = 0
   Private Const INDEX_SOURCE_POSTFIX_LENGTH As Integer = 1
   Private Const INDEX_SOURCE_PACKED_LENGTH As Integer = 2

   ''' <summary>
   ''' Maximum length of blinding bytes.
   ''' </summary>
   Private Const MAX_NORMAL_SINGLE_BLINDING_LENGTH As Integer = 15   ' This needs to be a power of 2 minus 1 so it can be used with an "And" operator

   ''' <summary>
   ''' Maximum value of the minimum length.
   ''' </summary>
   Private Const MAX_MINIMUM_LENGTH As Integer = 256
#End Region
#End Region

#Region "Public methods"
   '******************************************************************
   ' Public methods
   '******************************************************************

   ''' <summary>
   ''' Add blinders to a byte array
   ''' </summary>
   ''' <remarks>There may be no blinding, at all! I.e. the "blinded" array is the same as the source array
   '''  This behaviour is intentional. So an attacker will not known, whether there was blinding, or not.</remarks>
   ''' <param name="sourceBytes">Source bytes to add blinding to.</param>
   ''' <param name="minimumLength">Minimum length of blinded array.</param>
   ''' <returns>Blinded byte array.</returns>
   ''' <exception cref="ArgumentException">Thrown if minimum length is outside the allowed boundaries.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="sourceBytes"/> is <c>Nothing</c>.</exception>
   Public Shared Function BuildBlindedByteArray(sourceBytes As Byte(), minimumLength As Integer) As Byte()
      RequireNonNull(sourceBytes, NameOf(sourceBytes))

      CheckMinimumLength(minimumLength)

      Dim packedSourceLength As Byte() = PackedUnsignedInteger.FromInteger(sourceBytes.Length)

      Dim blindingLength As Integer() = GetBalancedBlindingLengths(packedSourceLength.Length, sourceBytes.Length, minimumLength)

      Dim prefixBlinding As Byte() = CreateBlinding(blindingLength(INDEX_LENGTHS_PREFIX_LENGTH))
      Dim postfixBlinding As Byte() = CreateBlinding(blindingLength(INDEX_LENGTHS_POSTFIX_LENGTH))

      Dim resultLength As Integer = LENGTHS_LENGTH + packedSourceLength.Length + prefixBlinding.Length + sourceBytes.Length + postfixBlinding.Length

      Dim result As Byte() = New Byte(0 To resultLength - 1) {}

      result(0) = CByte(prefixBlinding.Length)
      result(1) = CByte(postfixBlinding.Length)

      Dim resultIndex As Integer = LENGTHS_LENGTH

      resultIndex += CopyByteArrayAndZapSource(packedSourceLength, result, resultIndex)

      resultIndex += CopyByteArrayAndZapSource(prefixBlinding, result, resultIndex)

      resultIndex += CopyByteArray(sourceBytes, result, resultIndex)

      CopyByteArrayAndZapSource(postfixBlinding, result, resultIndex)

      Return result
   End Function


   ''' <summary>
   ''' Remove blinders from a byte array.
   ''' </summary>
   ''' <param name="sourceBytes">Blinded byte array.</param>
   ''' <returns>Byte array without blinders.</returns>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="sourceBytes"/> is not a valid blinded byte array.</exception>
   ''' <exception cref="ArgumentNullException">Thrown is <paramref name="sourceBytes"/> is <c>Nothing</c>.</exception>
   Public Shared Function UnBlindByteArray(sourceBytes As Byte()) As Byte()
      RequireNonNull(sourceBytes, NameOf(sourceBytes))

      If sourceBytes.Length > LENGTHS_LENGTH Then
         Dim packedNumberLength As Integer = PackedUnsignedInteger.GetExpectedLength(sourceBytes, INDEX_SOURCE_PACKED_LENGTH)

         If (sourceBytes(INDEX_SOURCE_PREFIX_LENGTH) >= 0) Then
            ' No. of bytes to skip is the blinding prefix length plus the two length bytes plus the source length
            Dim prefixBlindingLength As Integer = LENGTHS_LENGTH + sourceBytes(INDEX_SOURCE_PREFIX_LENGTH) + packedNumberLength

            If sourceBytes(INDEX_SOURCE_POSTFIX_LENGTH) >= 0 Then
               Dim postfixBlindingLength As Integer = sourceBytes(INDEX_SOURCE_POSTFIX_LENGTH)

               Dim totalBlindingsLength As Integer = prefixBlindingLength + postfixBlindingLength
               Dim dataLength As Integer = PackedUnsignedInteger.ToInteger(sourceBytes, INDEX_SOURCE_PACKED_LENGTH)

               ' The largest number in the following addition can only be just over 1,073,741,823
               ' This can never overflow into negative values
               If (totalBlindingsLength + dataLength) <= sourceBytes.Length Then _
                  Return ArrayHelper.CopyOf(sourceBytes, prefixBlindingLength, dataLength)
            End If
         End If
      End If

      Throw New ArgumentException(ERROR_MESSAGE_INVALID_ARRAY)
   End Function
#End Region

#Region "Private methods"
   '******************************************************************
   ' Private methods
   '******************************************************************

   ''' <summary>
   ''' Check the validity of the requested minimum length.
   ''' </summary>
   ''' <param name="minimumLength">Requested minimum length.</param>
   ''' <exception cref="ArgumentException">Thrown if minimum length is outside the allowed boundaries.</exception>
   Private Shared Sub CheckMinimumLength(minimumLength As Integer)
      If (minimumLength < 0) OrElse (minimumLength > MAX_MINIMUM_LENGTH) Then _
         Throw New ArgumentException(ERROR_MESSAGE_INVALID_MIN_LENGTH)
   End Sub

   ''' <summary>
   ''' Get the length for a blinding part.
   ''' </summary>
   ''' <returns>Length for blinding.</returns>
   Private Shared Function GetBlindingLength() As Integer
      Return SecurePseudoRandomNumberGenerator.GetInteger() And MAX_NORMAL_SINGLE_BLINDING_LENGTH
   End Function

   ''' <summary>
   ''' Adapt blinding lengths to minimum length.
   ''' </summary>
   ''' <param name="blindingLength"> Array of blinding lengths.</param>
   ''' <param name="sourceLengthLength">Length of the source length.</param>
   ''' <param name="sourceLength">Length of source.</param>
   ''' <param name="minimumLength">Required minimum length.</param>
   Private Shared Sub AdaptBlindingLengthsToMinimumLength(blindingLength As Integer(), sourceLengthLength As Integer, sourceLength As Integer, minimumLength As Integer)
      Dim combinedLength As Integer = LENGTHS_LENGTH + sourceLengthLength + blindingLength(INDEX_LENGTHS_PREFIX_LENGTH) + sourceLength + blindingLength(INDEX_LENGTHS_POSTFIX_LENGTH)

      If combinedLength < minimumLength Then
         Dim diff As Integer = minimumLength - combinedLength
         Dim halfDiff As Integer = diff >> 1

         blindingLength(INDEX_LENGTHS_PREFIX_LENGTH) += halfDiff
         blindingLength(INDEX_LENGTHS_POSTFIX_LENGTH) += halfDiff

         ' Adjust for odd difference
         If (diff And 1) <> 0 Then
            If (diff And 2) <> 0 Then
               blindingLength(INDEX_LENGTHS_PREFIX_LENGTH) += 1
            Else
               blindingLength(INDEX_LENGTHS_POSTFIX_LENGTH) += 1
            End If
         End If
      End If
   End Sub


   ''' <summary>
   ''' Create blinding lengths so that their combined lengths are at least minimum length.
   ''' </summary>
   ''' <param name="sourceLengthLength">Length of the source length.</param>
   ''' <param name="sourceLength">Length of source.</param>
   ''' <param name="minimumLength">Required minimum length.</param>
   ''' <returns>Array of blinding lengths.</returns>
   Private Shared Function GetBalancedBlindingLengths(sourceLengthLength As Integer, sourceLength As Integer, minimumLength As Integer) As Integer()
      Dim result As Integer() = New Integer(0 To LENGTHS_LENGTH - 1) {}

      result(INDEX_LENGTHS_PREFIX_LENGTH) = GetBlindingLength()
      result(INDEX_LENGTHS_POSTFIX_LENGTH) = GetBlindingLength()

      ' If minimumLength is greater than 0 adapt blinding lengths to be at least minimum length when combined.
      If minimumLength > 0 Then _
         AdaptBlindingLengthsToMinimumLength(result, sourceLengthLength, sourceLength, minimumLength)

      Return result
   End Function

   ''' <summary>
   ''' Copy a byte array into another byte array.
   ''' </summary>
   ''' <param name="sourceBytes">Source byte array.</param>
   ''' <param name="destinationBytes"> Destination byte array.</param>
   ''' <param name="startToIndex">Start index in the destination byte array.</param>
   ''' <returns>Length of the source bytes.</returns>
   Private Shared Function CopyByteArray(sourceBytes As Byte(), destinationBytes As Byte(), startToIndex As Integer) As Integer
      Array.Copy(sourceBytes, 0, destinationBytes, startToIndex, sourceBytes.Length)

      Return sourceBytes.Length
   End Function

   ''' <summary>
   ''' Copy a byte array into another byte array and fill the source byte array with 0s.
   ''' </summary>
   ''' <param name="sourceBytes">Source byte array.</param>
   ''' <param name="destinationBytes"> Destination byte array.</param>
   ''' <param name="startToIndex">Start index in the destination byte array.</param>
   ''' <returns>Length of the source bytes.</returns>
   Private Shared Function CopyByteArrayAndZapSource(sourceBytes As Byte(), destinationBytes As Byte(), startToIndex As Integer) As Integer
      Dim result As Integer = CopyByteArray(sourceBytes, destinationBytes, startToIndex)

      ArrayHelper.Clear(sourceBytes)

      Return result
   End Function

   ''' <summary>
   ''' Create a blinding array.
   ''' </summary>
   ''' <param name="blindingLength">Length of blinding array.</param>
   ''' <returns>Blinding array.</returns>
   Private Shared Function CreateBlinding(blindingLength As Integer) As Byte()
      Dim result = New Byte(0 To blindingLength - 1) {}

      SecurePseudoRandomNumberGenerator.GetBytes(result)

      Return result
   End Function

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