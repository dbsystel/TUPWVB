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
' Version: 1.1.1
'
' Change history:
'    2020-04-30: V1.0.0: Created.
'    2020-05-12: V1.1.0: Corrected handling of 0 length final block.
'    2020-06-18: V1.1.1: Corrected handling of null parameters in constructors.
'

Imports System.Security.Cryptography

''' <summary>
''' Implements a counter mode crypto transform.
''' </summary>
''' <remarks>
''' It is very strange that such a widely used crypto mode is not present in the .Net framework.
''' </remarks>
Public Class CounterModeCryptoTransform : Implements ICryptoTransform, IDisposable
#Region "Private constants"
   '
   ' Unfortunately VB has no way of specifying a byte constant literal.
   ' So we have to define byte constants here.
   '
   Private Const ZERO_AS_BYTE As Byte = 0US
   Private Const ONE_AS_BYTE As Byte = 1US
   Private Const FF_AS_BYTE As Byte = &HFFUS
#End Region

#Region "Instance variables"
   Private m_SymmetricAlgorithm As SymmetricAlgorithm
   Private m_Counter As Byte()
   Private m_CounterEncryptor As ICryptoTransform
   Private m_BlockSizeInBytes As Integer
   Private m_XorMask As Byte()
   Private m_XorMaskPosition As Integer
#End Region

#Region "Constructor"
   ''' <summary>
   ''' Initialize this instance.
   ''' </summary>
   ''' <param name="algorithmInstance">Instance of a <see cref="SymmetricAlgorithm"/>.</param>
   ''' <param name="key">Key for the <paramref name="algorithmInstance"/>.</param>
   ''' <param name="iv">Initialization vector for the <paramref name="algorithmInstance"/>.</param>
   ''' <exception cref="ArgumentException">Thrown if the size of the <paramref name="iv"/> is not the same as 
   ''' the block size of the <paramref name="algorithmInstance"/>.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if any of the parameters is <c>Nothing</c>.</exception>
   Public Sub New(algorithmInstance As SymmetricAlgorithm, key As Byte(), iv As Byte())
      ' Check parameters
      If algorithmInstance Is Nothing Then _
         Throw New ArgumentNullException(NameOf(algorithmInstance))

      InitializeInstance(algorithmInstance, key, iv)
   End Sub

   ''' <summary>
   ''' Create a new instance of this class.
   ''' </summary>
   ''' <param name="algorithmName">Name of the underlying cryptographic algorithm.</param>
   ''' <param name="key">Key for the algorithm.</param>
   ''' <param name="iv">Start value for counter (like an iv).</param>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="algorithmName"/> is not a valid algorithm name
   ''' or the size of the <paramref name="iv"/> is not the same as the block size of the 
   ''' <paramref name="algorithmName"/> instance.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if any of the paramters is <c>Nothing</c>.</exception>
   Public Sub New(algorithmName As String, key As Byte(), iv As Byte())
      If algorithmName Is Nothing Then _
         Throw New ArgumentNullException(NameOf(algorithmName))

      ' Create a private algorithm object from the algorithm name
      Dim algorithmInstance As SymmetricAlgorithm = SymmetricAlgorithm.Create(algorithmName)

      If algorithmInstance Is Nothing Then _
         Throw New ArgumentException(String.Format("algorithmName '{0}' is not a valid algorithm", algorithmName))

      InitializeInstance(algorithmInstance, key, iv)
   End Sub

#End Region

#Region "Public properties"
   ''' <summary>
   ''' Gets the input block size.
   ''' </summary>
   ''' <returns>Input block size.</returns>
   Public ReadOnly Property InputBlockSize As Integer Implements ICryptoTransform.InputBlockSize
      Get
         Return m_BlockSizeInBytes
      End Get
   End Property

   ''' <summary>
   ''' Gets the output block size.
   ''' </summary>
   ''' <returns>Output block size.</returns>
   Public ReadOnly Property OutputBlockSize As Integer Implements ICryptoTransform.OutputBlockSize
      Get
         Return m_BlockSizeInBytes
      End Get
   End Property

   ''' <summary>
   ''' Gets a value indicating whether multiple blocks can be transformed at once.
   ''' </summary>
   ''' <remarks>
   ''' This property indicates whether it is allowed to request the transformation of a byte array
   ''' that has a size that is a multiple of the block size.
   ''' </remarks>
   ''' <returns><c>True</c>, if multiple blocks can be transformed at once, <c>False</c>, if not.</returns>
   Public ReadOnly Property CanTransformMultipleBlocks As Boolean Implements ICryptoTransform.CanTransformMultipleBlocks
      Get
         Return True
      End Get
   End Property

   ''' <summary>
   ''' Gets a value indicating whether the current transform can be reused.
   ''' </summary>
   ''' <remarks>
   ''' This property always returns false as a reuse of a counter transform would completely destroy all security.
   ''' This is a "feature" of the counter mode.
   ''' </remarks>
   ''' <returns>><c>True</c>, if the current transform can be reused, <c>False</c>, if not.</returns>
   Public ReadOnly Property CanReuseTransform As Boolean Implements ICryptoTransform.CanReuseTransform
      Get
         Return False
      End Get
   End Property
#End Region

#Region "Public methods"
   ''' <summary>
   ''' Transforms the specified region of the input byte array and copies the resulting transform
   ''' to the specified region of the output byte array.
   ''' </summary>
   ''' <param name="inputBuffer">The input bytes for which to compute the transform.</param>
   ''' <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
   ''' <param name="inputCount">he number of bytes in the input byte array to use as data.</param>
   ''' <param name="outputBuffer">The output to which to write the transform.</param>
   ''' <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
   ''' <returns>The number of bytes written.</returns>
   ''' <exception cref="ArgumentException">Thrown if the block size is not a multiple of the transform block size, or a buffer
   ''' offset or count does not match the buffer size.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if either buffer is <c>Nothing</c>.</exception>
   ''' <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="inputOffset"/>, <paramref name="inputCount"/>,
   ''' <paramref name="outputOffset"/> are less than 0 or 
   ''' <paramref name="inputOffset"/> is larger than the length of <paramref name="inputBuffer"/> or 
   ''' <paramref name="outputOffset"/> is larger than the length of <paramref name="outputBuffer"/>. 
   ''' </exception>
   Public Function TransformBlock(inputBuffer() As Byte, inputOffset As Integer, inputCount As Integer, outputBuffer() As Byte, outputOffset As Integer) As Integer Implements ICryptoTransform.TransformBlock
      CheckBufferParameters("input", inputBuffer, inputOffset, inputCount)
      CheckBufferParameters("output", outputBuffer, outputOffset, inputCount)

      If inputCount Mod m_BlockSizeInBytes <> 0 Then _
         Throw New ArgumentException("inputCount is not a multiple of the transform block size")

      Dim sourceIndex As Integer = inputOffset
      Dim destinationIndex As Integer = outputOffset

      For i As Integer = 0 To inputCount - 1
         If m_XorMaskPosition >= m_XorMask.Length Then _
            EncryptCounterThenIncrement()

         outputBuffer(destinationIndex) = inputBuffer(sourceIndex) Xor m_XorMask(m_XorMaskPosition)

         sourceIndex += 1
         destinationIndex += 1
         m_XorMaskPosition += 1
      Next

      Return inputCount
   End Function

   ''' <summary>
   ''' Transforms the specified region of the specified byte array.
   ''' </summary>
   ''' <param name="inputBuffer">The input for which to compute the transform.</param>
   ''' <param name="inputOffset">The offset into the byte array from which to begin using data.</param>
   ''' <param name="inputCount">The number of bytes in the byte array to use as data.</param>
   ''' <returns>The computed transform.</returns>
   ''' <exception cref="ArgumentException">Thrown if offset or count does not match the input buffer size.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if input buffer is <c>Nothing</c>.</exception>
   ''' <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="inputOffset"/> or <paramref name="inputCount"/> are
   ''' less than 0 or <paramref name="inputOffset"/> is larger than the length of <paramref name="inputBuffer"/>.</exception>
   Public Function TransformFinalBlock(inputBuffer() As Byte, inputOffset As Integer, inputCount As Integer) As Byte() Implements ICryptoTransform.TransformFinalBlock
      CheckBufferParameters("input", inputBuffer, inputOffset, inputCount)

      Dim output As Byte()

      If inputCount > 0 Then
         output = New Byte(0 To m_BlockSizeInBytes - 1) {}

         TransformBlock(inputBuffer, inputOffset, inputCount, output, 0)
      Else
         output = Array.Empty(Of Byte)()
      End If

      Return output
   End Function
#End Region

#Region "Private methods"
#Region "Constructor helpers"
   ''' <summary>
   ''' Initialize this instance.
   ''' </summary>
   ''' <param name="algorithmInstance">Instance of a <see cref="SymmetricAlgorithm"/>.</param>
   ''' <param name="key">Key for the <paramref name="algorithmInstance"/>.</param>
   ''' <param name="iv">Initialization vector for the <paramref name="algorithmInstance"/>.</param>
   ''' <exception cref="ArgumentException">Thrown if the size of the <paramref name="iv"/> is not the same
   ''' as the block size of the <paramref name="algorithmInstance"/>.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> or <paramref name="iv"/> is <c>Nothing</c>.</exception>
   Private Sub InitializeInstance(algorithmInstance As SymmetricAlgorithm, key As Byte(), iv As Byte())
      If key Is Nothing Then _
         Throw New ArgumentNullException(NameOf(key))

      If iv Is Nothing Then _
         Throw New ArgumentNullException(NameOf(iv))

      m_SymmetricAlgorithm = algorithmInstance
      m_SymmetricAlgorithm.Mode = CipherMode.ECB
      m_SymmetricAlgorithm.Padding = PaddingMode.None

      ' Check if algorithm block size matches counter size
      m_BlockSizeInBytes = m_SymmetricAlgorithm.BlockSize >> 3  ' Convert algorithm block size to bytes.

      If iv.Length <> m_BlockSizeInBytes Then _
         Throw New ArgumentException(String.Format("Iv size ({0}) must be the same as the encryption algorithm ('{1}') block size ({2})",
                                     iv.Length,
                                     algorithmInstance.GetType().Name,
                                     m_BlockSizeInBytes))

      ' Create a private copy of the iv counter so that the counter source is not modified by counting
      m_Counter = CType(iv.Clone(), Byte())

      ' Create an encryptor from the algorithm
      m_CounterEncryptor = m_SymmetricAlgorithm.CreateEncryptor(key, iv)   ' Iv is not used for ECB mode, anyway.

      ' Allocate the xor mask and initialize the pointer
      m_XorMask = New Byte(0 To m_BlockSizeInBytes - 1) {}
      m_XorMaskPosition = m_BlockSizeInBytes    ' Position must point beyond end of mask so a new mask will be generated on first access
   End Sub
#End Region

#Region "Check methods"
   ''' <summary>
   ''' Checks the parameters for buffer handling.
   ''' </summary>
   ''' <param name="bufferName">Name of the buffer for use in exception messages.</param>
   ''' <param name="aBuffer">The buffer to check.</param>
   ''' <param name="offset">The offset into the buffer.</param>
   ''' <param name="count">Count of bytes.</param>
   ''' <exception cref="ArgumentException">Thrown if <paramref name="offset"/> and <paramref name="count"/>
   ''' do not match the <paramref name="aBuffer"/>.</exception>
   ''' <exception cref="ArgumentNullException">Thrown if <paramref name="aBuffer"/> is <c>Nothing</c>.</exception>
   ''' <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="offset"/> or <paramref name="count"/> are
   ''' less than 0 or <paramref name="offset"/> is larger than the length of <paramref name="aBuffer"/>.</exception>
   Private Shared Sub CheckBufferParameters(bufferName As String, aBuffer() As Byte, offset As Integer, count As Integer)
      If aBuffer Is Nothing Then _
         Throw New ArgumentNullException(bufferName & "Buffer")

      If offset < 0 Then _
         Throw New ArgumentOutOfRangeException(NameOf(offset))

      If aBuffer.Length > 0 AndAlso aBuffer.Length <= offset Then _
         Throw New ArgumentOutOfRangeException(NameOf(offset))

      If count < 0 Then _
         Throw New ArgumentOutOfRangeException(NameOf(count))

      If aBuffer.Length - offset < count Then _
         Throw New ArgumentException(String.Format("{0}Buffer is not large enough for data count from {0}Offset", bufferName))
   End Sub
#End Region

#Region "Encryption helpers"
   ''' <summary>
   ''' Encrypt the counter and increment it for the next encryption.
   ''' </summary>
   Private Sub EncryptCounterThenIncrement()
      m_CounterEncryptor.TransformBlock(m_Counter, 0, m_Counter.Length, m_XorMask, 0)
      IncrementCounter()

      m_XorMaskPosition = 0
   End Sub

   ''' <summary>
   ''' Increment counter.
   ''' </summary>
   Private Sub IncrementCounter()
      Dim counterValue As Byte
      Dim haveOverflow As Boolean = False

      '
      ' We need to increment a counter which is comprised of an arbitrary
      ' number of bytes. To achieve this, we start at the least significant
      ' byte and increment it. If it overflows, we continue with the next byte.
      ' If not, we are done.
      '
      For i As Integer = m_Counter.Length - 1 To 0 Step -1
         '
         ' 1. Increment current byte modulo 256.
         '
         counterValue = m_Counter(i)

         '
         ' VB does not allow overflowing, so we have to do the addition modulo 256 manually.
         '
         If counterValue = FF_AS_BYTE Then
            counterValue = ZERO_AS_BYTE

            '
            ' If the most significant byte overflows, we have to wrap to all 0 again.
            '
            If i = 0 Then _
               haveOverflow = True
         Else
            counterValue += ONE_AS_BYTE
         End If

         m_Counter(i) = counterValue

         '
         ' 2. If current byte is 0 after the increment, we are done,
         '
         If counterValue <> ZERO_AS_BYTE Then _
            Exit For
      Next

      '
      ' If we had an overflow, reset counter to 0.
      '
      If haveOverflow Then _
         ArrayHelper.Clear(m_Counter)
   End Sub
#End Region
#End Region

#Region "IDisposable Support"
   ''' <summary>
   ''' Marker, if disposition of managed resources has already been done.
   ''' </summary>
   Private m_IsDisposed As Boolean

   ''' <summary>
   ''' Dispose managed and unmanged resources.
   ''' </summary>
   ''' <param name="disposeManagedResources"><c>true</c>, if managed resource are to be disposed of, <c>false</c>, if not.</param>
   Protected Overridable Sub Dispose(disposeManagedResources As Boolean)
      If Not m_IsDisposed Then
         If disposeManagedResources Then

            '
            ' Disposing of resources needs to be synchronized to prevent a race condition.
            '
            SyncLock m_SymmetricAlgorithm
               m_IsDisposed = True

               m_XorMaskPosition = 0

               ArrayHelper.Clear(m_Counter)
               ArrayHelper.Clear(m_XorMask)

               m_CounterEncryptor.Dispose()
               m_SymmetricAlgorithm.Dispose()
            End SyncLock
         End If

         ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
         ' TODO: set large fields to null.
      End If
   End Sub

   ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
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
      ' TODO: uncomment the following line if Finalize() is overridden above.
      ' GC.SuppressFinalize(Me)
   End Sub
#End Region
End Class
