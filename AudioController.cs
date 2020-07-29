using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MelodyMaker
{
    [Route("api/[controller]")]
    public class AudioController : Controller
    {
        private readonly IHostingEnvironment _env;

        public AudioController(IHostingEnvironment env)
        {
            _env = env;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromForm]string phoneNumber)
        {
            //1) split number to to get 10 numbers in array
            var numsInPhone = phoneNumber.ToCharArray();
            //var chunkAudioFiles = new AudioFileReader[10];
            var chunkAudioStereo = new ISampleProvider[10];
            var backGroundMerger = new ISampleProvider[2];

            string contentRootPath = _env.ContentRootPath;
            string webRootPath = _env.WebRootPath;

            //2) iterate over numbers to read file by number name and form audio
            var k = 0;
            foreach (var num in numsInPhone)
            {
                //chunkAudioFiles.Append(new AudioFileReader(webRootPath + "\\app\\AudioFiles\\file_" +num+".wav"));

                var audio = new WdlResamplingSampleProvider(new AudioFileReader(webRootPath + "\\app\\AudioFiles\\file_" + num + ".mp3"), 44100);
                //chunkAudioFiles[k] = new AudioFileReader(webRootPath + "\\app\\AudioFiles\\file_" + num + ".mp3");
                chunkAudioStereo[k] = audio.ToStereo();
                k++;
            }
            byte[] buffer = new byte[1024];
            WaveFileWriter waveFileWriter = null;
            var timeRange = new double[10];
            string outputFile = webRootPath + "\\app\\AudioFiles\\output.wav";
            try
            {
                var counter = 0;
                foreach (var num in numsInPhone)
                {
                    WaveFileReader reader = new WaveFileReader(webRootPath + "\\app\\AudioFiles\\file_" + num + ".wav");
                    if (waveFileWriter == null)
                    {
                        // first time in create new Writer
                        waveFileWriter = new WaveFileWriter(outputFile, reader.WaveFormat);
                    }
                    else
                    {
                        if (!reader.WaveFormat.Equals(waveFileWriter.WaveFormat))
                        {
                            var audio = new WdlResamplingSampleProvider(new AudioFileReader(webRootPath + "\\app\\AudioFiles\\file_" + num + ".wav"), 44100);
                            reader = (WaveFileReader)audio.ToStereo();
                        }
                    }
                    timeRange[counter] = reader.TotalTime.TotalSeconds;
                    int read;
                    if (waveFileWriter.WaveFormat.BitsPerSample == 24)
                    {
                        while ((read = reader.Read(buffer, 0, buffer.Length - (buffer.Length % waveFileWriter.WaveFormat.BlockAlign))) > 0)
                        {
                            waveFileWriter.Write(buffer, 0, read);
                        }
                    }
                    else
                    {
                        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            waveFileWriter.Write(buffer, 0, read);
                        }
                    }

                    counter++;
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (waveFileWriter != null)
                {
                    waveFileWriter.Dispose();
                }
            }
            backGroundMerger[0] = new WdlResamplingSampleProvider(new AudioFileReader(webRootPath + "\\app\\AudioFiles\\output.wav"), 44100).ToStereo();
            backGroundMerger[1] = new WdlResamplingSampleProvider(new AudioFileReader(webRootPath + "\\app\\AudioFiles\\harmony.wav"), 44100).ToStereo();
            // 3) Merge Audios
            var mixer = new MixingSampleProvider(backGroundMerger);
            WaveFileWriter.CreateWaveFile16("mixed.wav", mixer);

            //4) TODO : Merge background audio
            //5) TODO : Save mobile number somewhere

            //6) Return generated audio file
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }


        public static void Combine(string[] inputFiles, Stream output)
        {
            foreach (string file in inputFiles)
            {
                Mp3FileReader reader = new Mp3FileReader(file);
                if ((output.Position == 0) && (reader.Id3v2Tag != null))
                {
                    output.Write(reader.Id3v2Tag.RawData, 0, reader.Id3v2Tag.RawData.Length);
                }
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                {
                    output.Write(frame.RawData, 0, frame.RawData.Length);
                }
            }
        }
    }
}
