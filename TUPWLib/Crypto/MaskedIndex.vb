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
' Version: 1.3.0
'
' Change history:
'    2020-05-28: V1.0.0: Created.
'    2020-06-15: V1.0.1: Clear key bytes.
'    2022-11-07: V1.1.0: Better mixing of bytes from and to buffers.
'    2022-11-08: V1.2.0: Name all constants.
'    2022-11-22: V1.3.0: Use AES.Create() for cipher creation.
'

Option Strict On
Option Explicit On

Imports System.Security.Cryptography

''' <summary>
''' Create masks for indices
''' </summary>
Public Class MaskedIndex : Implements IDisposable
#Region "Private constants"
   ''' <summary>
   ''' Key size
   ''' </summary>
   Private Const KEY_SIZE As Integer = 16

   ''' <summary>
   ''' Maximum key index
   ''' </summary>
   Private Const MAX_KEY_INDEX As Integer = KEY_SIZE - 1

   ''' <summary>
   ''' Buffer size
   ''' </summary>
   Private Const BUFFER_SIZE As Integer = 16

   ''' <summary>
   ''' Maximum buffer index
   ''' </summary>
   Private Const MAX_BUFFER_INDEX As Integer = BUFFER_SIZE - 1

   ''' <summary>
   ''' Mask for additions modulo buffer size
   ''' </summary>
   Private Const BUFFER_SIZE_MASK As Integer = MAX_BUFFER_INDEX

   ''' <summary>
   ''' Modulo value for the offset of an integer in a buffer
   ''' </summary>
   Private Const MOD_BUFFER_SIZE_FOR_INTEGER As Integer = BUFFER_SIZE - 3

   ''' <summary>
   ''' Byte value to prime buffer with
   ''' </summary>
   Private Const BUFFER_PRIMER As Byte = &H5A

   ''' <summary>
   ''' Step size for setting and getting bytes in the buffer
   ''' </summary>
   Private Const STEP_SIZE As Integer = 3

   ''' <summary>
   ''' Number of bits to shift for a byte shift
   ''' </summary>
   Private Const BYTE_SHIFT As Integer = 8

   ''' <summary>
   ''' Byte mask for integers
   ''' </summary>
   Private Const INTEGER_BYTE_MASK As Integer = &HFF

   ''' <summary>
   ''' Maximum allowed integer mask
   ''' </summary>
   Private Const MAX_INTEGER_MASK As Integer = &H7FFF_FFFF
#End Region

#Region "Instance variables"
   '******************************************************************
   ' Instance variables
   '******************************************************************

   ''' <summary>
   ''' Private encryptor
   ''' </summary>
   Private m_Encryptor As ICryptoTransform

   ''' <summary>
   ''' Source buffer for mask generation
   ''' </summary>
   Private ReadOnly m_SourceBuffer As Byte() = New Byte(0 To MAX_BUFFER_INDEX) {}

   ''' <summary>
   ''' Buffer for encryption result
   ''' </summary>
   Private ReadOnly m_MaskBuffer As Byte() = New Byte(0 To MAX_BUFFER_INDEX) {}
#End Region

#Region "Constructor"
   '******************************************************************
   ' Constructor
   '******************************************************************

   ''' <summary>
   ''' Constructor
   ''' </summary>
   Public Sub New()
      InitializeCipher()
   End Sub
#End Region

#Region "Public methods"
   '******************************************************************
   ' Public methods
   '******************************************************************

   ''' <summary>
   ''' Get an integer mask for an index
   ''' </summary>
   ''' 
   ''' <param name="forIndex">The index to use</param>
   ''' <returns>The <c>Integer</c> mask for the given index</returns>
   Public Function GetIntegerMask(forIndex As Integer) As Integer
      Dim sanitizedIndex As Integer = forIndex And MAX_INTEGER_MASK

      GetMaskBuffer(sanitizedIndex)

      Dim result As Integer = GetMaskIntegerFromArray(m_MaskBuffer,
                                                      (7 * (sanitizedIndex Mod MOD_BUFFER_SIZE_FOR_INTEGER) + 3) Mod MOD_BUFFER_SIZE_FOR_INTEGER)

      ArrayHelper.Clear(m_MaskBuffer)

      Return result
   End Function

   ''' <summary>
   ''' Get a byte mask for an index
   ''' </summary>
   ''' 
   ''' <param name="forIndex">The index to use</param>
   ''' <returns>The <c>Byte</c> mask for the given index</returns>
   Public Function GetByteMask(forIndex As Integer) As Byte
      Dim sanitizedIndex As Integer = forIndex And MAX_INTEGER_MASK

      GetMaskBuffer(sanitizedIndex)

      ' No need for "Math.Abs" as "And 15" removes the sign
      Dim result As Byte = m_MaskBuffer((13 * (sanitizedIndex And BUFFER_SIZE_MASK) + 5) And BUFFER_SIZE_MASK)

      ArrayHelper.Clear(m_MaskBuffer)

      Return result
   End Function
#End Region

#Region "Private methods"
   '******************************************************************
   ' Private methods
   '******************************************************************

   ''' <summary>
   ''' Initialize the cipher
   ''' </summary>
   Private Sub InitializeCipher()
      Dim key As Byte() = New Byte(0 To MAX_KEY_INDEX) {}

      SecurePseudoRandomNumberGenerator.GetBytes(key)

      Using cipher As Aes = Aes.Create()
         With cipher
            .Mode = CipherMode.ECB
            .Padding = PaddingMode.None
            .Key = key
         End With

         m_Encryptor = cipher.CreateEncryptor()
      End Using

      ArrayHelper.Clear(key)
   End Sub

   ''' <summary>
   ''' Calculate a buffer full of mask bytes
   ''' </summary>
   ''' <param name="sanitizedIndex">Sanitized index to use for mask calculation</param>
   Private Sub GetMaskBuffer(sanitizedIndex As Integer)
      ArrayHelper.Fill(m_SourceBuffer, BUFFER_PRIMER)

      Dim offset As Integer = (11 * (sanitizedIndex Mod MOD_BUFFER_SIZE_FOR_INTEGER) + 2) Mod MOD_BUFFER_SIZE_FOR_INTEGER
      StoreIntegerInArray(sanitizedIndex, m_SourceBuffer, offset)

      m_Encryptor.TransformBlock(m_SourceBuffer, 0, m_SourceBuffer.Length, m_MaskBuffer, 0)

      ArrayHelper.Clear(m_SourceBuffer)
   End Sub

   ''' <summary>
   ''' Stores the bytes of an integer in an existing array
   ''' </summary>
   ''' <param name="sourceInt">Integer to convert</param>
   ''' <param name="destArray">Destination array</param>
   ''' <param name="startPos">Start position in the <paramref name="destArray"/></param>
   Private Shared Sub StoreIntegerInArray(sourceInt As Integer, ByRef destArray As Byte(), startPos As Integer)
      Dim toPos As Integer = startPos
      Dim work As Integer = sourceInt

      destArray(toPos) = CByte(work And INTEGER_BYTE_MASK)

      toPos = (toPos + STEP_SIZE) And BUFFER_SIZE_MASK
      work >>= BYTE_SHIFT
      destArray(toPos) = CByte(work And INTEGER_BYTE_MASK)

      toPos = (toPos + STEP_SIZE) And BUFFER_SIZE_MASK
      work >>= BYTE_SHIFT
      destArray(toPos) = CByte(work And INTEGER_BYTE_MASK)

      toPos = (toPos + STEP_SIZE) And BUFFER_SIZE_MASK
      work >>= BYTE_SHIFT
      destArray(toPos) = CByte(work And INTEGER_BYTE_MASK)
   End Sub

   ''' <summary>
   ''' Get a mask integer from the bytes in an array
   ''' </summary>
   ''' <param name="sourceArray">Byte array to get the integer from</param>
   ''' <param name="startPos">Start position in the byte array</param>
   ''' <returns>Mask integer</returns>
   Private Shared Function GetMaskIntegerFromArray(sourceArray As Byte(), startPos As Integer) As Integer
      Dim result As Integer = 0
      Dim fromPos As Integer = startPos

      result = sourceArray(fromPos)

      result <<= BYTE_SHIFT
      fromPos = (fromPos + STEP_SIZE) And BUFFER_SIZE_MASK
      result = result Or (sourceArray(fromPos) And INTEGER_BYTE_MASK)

      result <<= BYTE_SHIFT
      fromPos = (fromPos + STEP_SIZE) And BUFFER_SIZE_MASK
      result = result Or (sourceArray(fromPos) And INTEGER_BYTE_MASK)

      result <<= BYTE_SHIFT
      fromPos = (fromPos + STEP_SIZE) And BUFFER_SIZE_MASK
      result = result Or (sourceArray(fromPos) And INTEGER_BYTE_MASK)

      Return result
   End Function
#End Region

#Region "IDisposable Support"
   '******************************************************************
   ' IDisposable support
   '******************************************************************

   ''' <summary>
   ''' Object only used for locking the call to Dispose.
   ''' </summary>
   Private ReadOnly m_LockObject As New Object

   ''' <summary>
   ''' Marker, if disposition of managed resources has already been done.
   ''' </summary>
   Private m_IsDisposed As Boolean = False

   ''' <summary>
   ''' Dispose managed and unmanged resources.
   ''' </summary>
   ''' <param name="disposeManagedResources"><c>true</c>, if managed resource are to be disposed of, <c>false</c>, if not.</param>
   Protected Overridable Sub Dispose(disposeManagedResources As Boolean)
      '
      ' Disposing of resources needs to be synchronized to prevent a race condition.
      '
      SyncLock m_LockObject
         If Not m_IsDisposed Then
            m_IsDisposed = True

            If disposeManagedResources Then _
               m_Encryptor.Dispose()

            ' Free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' Set large fields to null.
         End If
      End SyncLock
   End Sub

   ' Override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
   'Protected Overrides Sub Finalize()
   '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
   '    Dispose(False)
   '    MyBase.Finalize()
   'End Sub

   ''' <summary>
   ''' Dispose of resources.
   ''' </summary>
   ''' <remarks>
   ''' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
   ''' </remarks>
   Public Sub Dispose() Implements IDisposable.Dispose
      Dispose(True)
      ' Uncomment the following line if Finalize() is overridden above.
      ' GC.SuppressFinalize(Me)
   End Sub

   ''' <summary>
   ''' Checks whether this instance is valid
   ''' </summary>
   ''' <returns><c>true</c>, if this instance is in a valid state, <c>false</c>, if this instance has already been disposed of.</returns>
   Public ReadOnly Property IsValid As Boolean
      Get
         SyncLock m_LockObject
            Return Not m_IsDisposed
         End SyncLock
      End Get
   End Property
#End Region
End Class
