/* 
 * Copyright (c) 2011-2018 FEELTeam - Maurizio Montel.
 * 
 * This file is part of the FEEL (FrontEnd - Emulator Launcher) distribution.
 *   (https://github.com/dr-prodigy/feel-frontend)
 * 
 * FEEL is free software: you can redistribute it and/or modify  
 * it under the terms of the GNU Lesser General Public License as   
 * published by the Free Software Foundation, version 3.
 *
 * FEEL is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 *
 * doc/info/contacts: http://feelfrontend.altervista.org
 */

using System.IO;
using System;
using System.Windows.Forms;
using SharpDX;
using SharpDX.DirectSound;
using SharpDX.Multimedia;

namespace feel
{
    class CSound
    {
        private static DirectSound _dSound = null;
        //private Device _dSound;
        //private Microsoft.DirectX.DirectSound.Buffer _buffer;
        private SecondarySoundBuffer _buffer;
        //private BufferPlayFlags _playFlag;
        private PlayFlags _playFlag;
        private int _position;

        public CSound(string fileName, int volume, Form controlForm)
        {
            try
            {
                if (_dSound == null)
                    _dSound = new DirectSound();
                _dSound.SetCooperativeLevel(controlForm.Handle, CooperativeLevel.Priority);
                SetSound(fileName, volume);
            }
            catch
            {
                throw new Exception("Sound-card is missing: aborting");
            }
        }

        ~CSound()
        {
            Dispose();
        }

        public string KeyName { get; set; }

        public string FileName { get; set; }

        public void SetSound(string fileName, int volume)
        {
            if (FileName == fileName)
                return;

            FileName = fileName;
            if (File.Exists(FileName))
            {
                var desc = new SoundBufferDescription();
                desc.Flags = BufferFlags.GetCurrentPosition2 | BufferFlags.ControlFrequency | BufferFlags.ControlPan |
                    BufferFlags.ControlVolume | BufferFlags.GlobalFocus;

                // Open the wave file in binary. 
                var reader = new BinaryReader(File.OpenRead(fileName));
                // Read in the wave file header.
                var chunkId = new string(reader.ReadChars(4));
                var chunkSize = reader.ReadInt32();
                var format = new string(reader.ReadChars(4));
                var subChunkId = new string(reader.ReadChars(4));
                var subChunkSize = reader.ReadInt32();
                var audioFormat = (WaveFormatEncoding)reader.ReadInt16();
                var numChannels = reader.ReadInt16();
                var sampleRate = reader.ReadInt32();
                var bytesPerSecond = reader.ReadInt32();
                var blockAlign = reader.ReadInt16();
                var bitsPerSample = reader.ReadInt16();
                var dataChunkId = new string(reader.ReadChars(4));
                var dataSize = reader.ReadInt32();

                // Check that the chunk ID is the RIFF format
                // and the file format is the WAVE format
                // and sub chunk ID is the fmt format
                // and the audio format is PCM
                // and the wave file was recorded in stereo format
                // and there is the data chunk header.
                // Otherwise return false.
                if (chunkId != "RIFF" || format != "WAVE" || subChunkId.Trim() != "fmt" || audioFormat != WaveFormatEncoding.Pcm || dataChunkId != "data" || dataSize < 0)
                {
                    _buffer = null;
                    return;
                }

                // Set the buffer description of the secondary sound buffer that the wave file will be loaded onto and the wave format.
                var bufferDesc = new SoundBufferDescription();
                bufferDesc.Flags = BufferFlags.GetCurrentPosition2 | BufferFlags.ControlPositionNotify | BufferFlags.GlobalFocus |
                                            BufferFlags.ControlVolume | BufferFlags.StickyFocus;
                bufferDesc.BufferBytes = dataSize;
                bufferDesc.Format = new WaveFormat(sampleRate, bitsPerSample, numChannels);
                bufferDesc.AlgorithmFor3D = Guid.Empty;

                // Create a temporary sound buffer with the specific buffer settings.
                _buffer = new SecondarySoundBuffer(_dSound, bufferDesc);
                // Read in the wave file data into the temporary buffer.
                var waveData = reader.ReadBytes(dataSize);

                // Close the reader
                reader.Close();

                // Lock the secondary buffer to write wave data into it.
                DataStream waveBufferData2;
                var waveBufferData1 = _buffer.Lock(0, dataSize, LockFlags.None, out waveBufferData2);

                // Copy the wave data into the buffer.
                waveBufferData1.Write(waveData, 0, dataSize);

                // Unlock the secondary buffer after the data has been written to it.
                _buffer.Unlock(waveBufferData1, waveBufferData2);
                
                SetVolume(volume);
            }
            else
                _buffer = null;
        }

        public bool Play()
        {
            if (_buffer != null)
            {
                _playFlag = PlayFlags.None;
                _buffer.Play(0, _playFlag);
                return true;
            }
            return false;
        }

        public void PlayLooping()
        {
            if (_buffer != null)
            {
                _playFlag = PlayFlags.Looping;
                _buffer.CurrentPosition = 0;
                _buffer.Play(0, _playFlag);
            }
        }

        public void Pause()
        {
            if (_buffer != null)
            {
                _buffer.Stop();
                int currentPlayWR;
                _buffer.GetCurrentPosition(out _position, out currentPlayWR);
            }
        }

        public void Resume()
        {
            if (_buffer != null)
            {
                _buffer.CurrentPosition = _position;
                _buffer.Play(0, _playFlag);
            }
        }

        public void Stop()
        {
            if (_buffer != null)
                _buffer.Stop();
        }

        public void StopLooping()
        {
            if (_buffer != null)
            {
                _buffer.Stop();
            }
        }

        public int Volume { get { return _buffer != null ? _buffer.Volume : 0; } set { SetVolume(value); } }

        private void SetVolume(int percentage)
        {
            if (_buffer != null)
            {
                if (percentage <= 0)
                {
                    _buffer.Volume = -10000;
                    return;
                }
                if (percentage >= 100)
                {
                    _buffer.Volume = 0;
                    return;
                }
                _buffer.Volume = (int)((float)(Math.Log10((float)percentage / 100.0f)) * 3321.928094887f);
            }
        }

        public void Dispose()
        {
            if (_buffer != null)
                _buffer.Dispose();
        }
    }
}
