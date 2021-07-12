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
' Version: 1.0.1
'
' Change history:
'    2020-05-28: V1.0.0: Created.
'    2020-06-15: V1.0.1: Clear key bytes.
'

Imports System.Security.Cryptography

''' <summary>
''' Create masks for indices
''' </summary>
Public Class MaskedIndex : Implements IDisposable
   Private Const BYTE_MASK As Integer = &HFF
   Private Const BYTE_PRIMER As Byte = &H5A
   Private Const SOURCE_BUFFER_INDEX_START As Integer = 6

#Region "Instance variables"
   '******************************************************************
   ' Instance variables
   '******************************************************************

   Private m_Encryptor As ICryptoTransform

   Private ReadOnly m_SourceBuffer As Byte() = New Byte(0 To 15) {}
   Private ReadOnly m_MaskBuffer As Byte() = New Byte(0 To 15) {}
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
      GetMaskBuffer(forIndex)

      Dim result As Integer = BitConverter.ToInt32(m_MaskBuffer, (7 * (Math.Abs(forIndex) Mod 13) + 3) Mod 13)

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
      GetMaskBuffer(forIndex)

      ' No need for "Math.Abs" as "And 15" removes the sign
      Dim result As Byte = m_MaskBuffer((13 * (forIndex And 15) + 5) And 15)

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
      Dim key As Byte() = New Byte(0 To 15) {}

      SecurePseudoRandomNumberGenerator.GetBytes(key)

      Using cipher As New AesCng With {
         .Mode = CipherMode.ECB,
         .Padding = PaddingMode.None,
         .Key = key
      }
         m_Encryptor = cipher.CreateEncryptor()
      End Using

      ArrayHelper.Clear(key)
   End Sub

   ''' <summary>
   ''' Calculate a buffer full of mask bytes
   ''' </summary>
   ''' <param name="forIndex">The index to use for mask calculation</param>
   Private Sub GetMaskBuffer(forIndex As Integer)
      ArrayHelper.Fill(m_SourceBuffer, BYTE_PRIMER)

      IntToBytes(forIndex, m_SourceBuffer, SOURCE_BUFFER_INDEX_START)

      m_Encryptor.TransformBlock(m_SourceBuffer, 0, m_SourceBuffer.Length, m_MaskBuffer, 0)

      ArrayHelper.Clear(m_SourceBuffer)
   End Sub

   ''' <summary>
   ''' Converts an <c>Integer</c> into an *existing* byte array
   ''' </summary>
   ''' <param name="i">Integer to convert</param>
   ''' <param name="destArray">Destination array</param>
   ''' <param name="startPos">Start position in the <paramref name="destArray"/></param>
   Private Shared Sub IntToBytes(i As Integer, ByRef destArray As Byte(), startPos As Integer)
      destArray(startPos) = CByte((i >> 24) And BYTE_MASK)
      destArray(startPos + 1) = CByte((i >> 16) And BYTE_MASK)
      destArray(startPos + 2) = CByte((i >> 8) And BYTE_MASK)
      destArray(startPos + 3) = CByte(i And BYTE_MASK)
   End Sub
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
