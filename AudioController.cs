using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NAudio.Lame;
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
        public async Task<IActionResult> Post([FromForm]string phoneNumber)
        {
            //1) split number to to get 10 numbers in array
            var numsInPhone = phoneNumber.ToCharArray();
            //var chunkAudioFiles = new AudioFileReader[10];
            var chunkAudioStereo = new ISampleProvider[10];
            var backGroundMerger = new ISampleProvider[2];

            string contentRootPath = _env.ContentRootPath;
            string webRootPath = _env.WebRootPath;

            byte[] buffer = new byte[1024];
            WaveFileWriter waveFileWriter = null;
            var timeRange = new double[10];
            string outputFile = webRootPath + "\\app\\AudioFiles\\output.wav";
            //string outputFile = contentRootPath + "\\output.wav";
            try
            {
                var counter = 0;
                foreach (var num in numsInPhone)
                {
                    //if (counter > 5)
                       // continue;

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

            //var sourceFiles = new List<string>();

            //foreach (var num in numsInPhone)
            //{
            //    sourceFiles.Add(webRootPath + "\\app\\AudioFiles\\file_" + num + ".wav");
            //}

            //Concatenate(outputFile, sourceFiles);

            backGroundMerger[0] = new WdlResamplingSampleProvider(new AudioFileReader(webRootPath + "\\app\\AudioFiles\\output.wav"), 44100).ToStereo();
            backGroundMerger[1] = new WdlResamplingSampleProvider(new AudioFileReader(webRootPath + "\\app\\AudioFiles\\harmony.wav"), 44100).ToStereo();


            var finalAudioName = "YourTone.wav";
            // 3) Merge Audios
            var mixer = new MixingSampleProvider(backGroundMerger);
            WaveFileWriter.CreateWaveFile16(finalAudioName, mixer);


            //TODO : Uncomment below and test after 16 bit audio are received
            ConvertWavToMp3(finalAudioName, "mixed.mp3");


            //TODO :  Use generated mp3 after 16 bit audio
            // var fileStream = await System.IO.File.ReadAllBytesAsync(finalAudioName);

            //// var fileStream = await System.IO.File.ReadAllBytesAsync("mp3_sample.mp3");

            // if (fileStream == null)
            //     return NotFound(); // returns a NotFoundResult with Status404NotFound response.

            // return new FileContentResult(fileStream, "audio/mpeg")
            // {
            //     FileDownloadName = outputFile
            // };


            var memory = new MemoryStream();
            using (var stream = new FileStream("mixed.mp3", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "audio/mpeg", $"mp3_sample.mp3", true);

        }

        [Obsolete]
        public static void Concatenate(string outputFile, IEnumerable<string> sourceFiles)
        {
            byte[] buffer = new byte[1024];
            WaveFileWriter waveFileWriter = null;

            try
            {
                foreach (string sourceFile in sourceFiles)
                {
                    using (WaveFileReader reader = new WaveFileReader(sourceFile))
                    {
                        if (waveFileWriter == null)
                        {
                            // first time in create new Writer
                            waveFileWriter = new WaveFileWriter(outputFile, reader.WaveFormat);
                        }
                        else
                        {
                            if (!reader.WaveFormat.Equals(waveFileWriter.WaveFormat))
                            {
                                throw new InvalidOperationException("Can't concatenate WAV Files that don't share the same format");
                            }
                        }


                        //while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                        //using (int read = reader.Read(buffer, 0, buffer.Length - (buffer.Length % waveFileWriter.WaveFormat.BlockAlign)))
                        int read = reader.Read(buffer, 0, buffer.Length - (buffer.Length % waveFileWriter.WaveFormat.BlockAlign));

                        waveFileWriter.WriteData(buffer, 0, read);

                    }
                }
            }
            finally
            {
                if (waveFileWriter != null)
                {
                    waveFileWriter.Dispose();
                }
            }

        }
        public static void ConvertWavToMp3(string WavFile, string outPutFile)
        {
            //CheckAddBinPath();
            WaveFileReader rdr = new WaveFileReader(WavFile);
            using (var wtr = new LameMP3FileWriter(outPutFile, rdr.WaveFormat, 128))
            {
                rdr.CopyTo(wtr);
                rdr.Dispose();
                wtr.Dispose();
                return;
            }
        }

        
    }
}
