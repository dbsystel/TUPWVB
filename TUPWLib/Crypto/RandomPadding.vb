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
' Version: 2.0.1
'
' Change history:
'    2020-04-23: V1.0.0: Created.
'    2020-05-27: V2.0.0: Removed unnecessary "RemovePadding" method.
'    2020-05-29: V2.0.1: Added exception comments.
'

''' <summary>
''' Random padding for block ciphers.
''' </summary>
''' <remarks>
''' Every "padding" class ought to have a "RemovePadding" method. It is part of the padding interface.
''' However, random padding can not be removed so this interface method is intentionally missing from this type of padding.
''' </remarks>
Public NotInheritable Class RandomPadding
#Region "Private constants"
   '******************************************************************
   ' Private constants
   '******************************************************************

   ''' <summary>
   ''' Maximum block size (64 KiB)
   ''' </summary>
   Private Const MAX_BLOCK_SIZE As Integer = 64 * 1024
#End Region


#Region "Public methods"
   '******************************************************************
   ' Public methods
   '******************************************************************

   ''' <summary>
   ''' Add padding bytes to source data.
   ''' </summary>
   ''' <param name="unpaddedSourceData">Data to be padded.</param>
   ''' <param name="blockSize">Block size in bytes.</param>
   ''' <returns>Data with padding bytes added.</returns>
   ''' <exception cref="ArgumentException">Thrown if block size is less than or equal to 0 or greater than <see cref="MAX_BLOCK_SIZE" />.</exception>
   ''' <exception cref="ArgumentNullException">Throw if <paramref name="unpaddedSourceData"/> is <c>Nothing</c>.</exception>
   Public Shared Function AddPadding(unpaddedSourceData As Byte(), blockSize As Integer) As Byte()
      RequireNonNull(unpaddedSourceData, NameOf(unpaddedSourceData))

      CheckBlockSize(blockSize)

      ' Get padding size
      Dim paddingLength As Integer = GetPaddingLength(unpaddedSourceData.Length, blockSize)

      ' Create padded byte array
      Dim result As Byte() = ArrayHelper.CopyOf(unpaddedSourceData, unpaddedSourceData.Length + paddingLength)

      ' Pad array with random bytes
      If paddingLength > 0 Then _
         SecurePseudoRandomNumberGenerator.GetBytes(result, unpaddedSourceData.Length, paddingLength)

      Return result
   End Function
#End Region

#Region "Private methods"
   '******************************************************************
   ' Private methods
   '******************************************************************

#Region "Check methods"
   ''' <summary>
   ''' Check block size.
   ''' </summary>
   ''' <param name="blockSize">Block size.</param>
   ''' <exception cref="ArgumentException">Thrown if block size is less than or equal to 0 or greater than <see cref="MAX_BLOCK_SIZE" />.</exception>
   Private Shared Sub CheckBlockSize(blockSize As Integer)
      If blockSize <= 0 Then _
         Throw New ArgumentException("Block size is less than or equal to 0")

      If blockSize > MAX_BLOCK_SIZE Then _
         Throw New ArgumentException("Block size must not be greater than " & MAX_BLOCK_SIZE.ToString())
   End Sub
#End Region

#Region "Padding handling"
   ''' <summary>
   ''' Calculate the padding length.
   ''' </summary>
   ''' <remarks>
   ''' If the unpadded data already have a length that is a multiple of 'blockSize'
   ''' an additional block with only padding bytes is added.
   ''' </remarks>
   ''' <param name="unpaddedLength">Length of the unpadded data.</param>
   ''' <param name="blockSize">lock size of which the padding size has to be a multiple.</param>
   ''' <returns>Padding length that brings the total length to a multiple of <paramref name="blockSize"/>.</returns>
   Private Shared Function GetPaddingLength(unpaddedLength As Integer, blockSize As Integer) As Integer
      Return blockSize - (unpaddedLength Mod blockSize)
   End Function
#End Region

#Region "Exception helpers"
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
