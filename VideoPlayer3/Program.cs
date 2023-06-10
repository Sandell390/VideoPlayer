


#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using Xabe.FFmpeg;

using static System.Net.Mime.MediaTypeNames;
using Image = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.Advanced;

Xabe.FFmpeg.FFmpeg.SetExecutablesPath("C:\\Users\\gummi\\Desktop\\ffmpeg\\bin");

string fileNameNo = @"C:\video\Images\image";
            
Func<string,string> outputFileNameBuilder = number =>
{
    number = "_%05d";
    return fileNameNo + number + ".png";
};




// string output = Path.ChangeExtension(@"C:\video\video1234", ".mp4");
// string input = @"C:\video\video4.mp4";
//
// var conversion = await FFmpeg.Conversions.FromSnippet.ChangeSize(input, output, VideoSize.Sqcif);
//
// IConversionResult result = await conversion.Start();
//
// Console.WriteLine("DOne");
// Console.ReadKey();
//             
IMediaInfo info = await FFmpeg.GetMediaInfo(@"C:\video\video.mp4");
IVideoStream? videoStream = info.VideoStreams.First()?.SetCodec(VideoCodec.png);

IConversionResult conversionResult = await FFmpeg.Conversions.New()
   .AddStream(videoStream)
   .ExtractEveryNthFrame(1, outputFileNameBuilder)
   .Start();

IAudioStream? audioStream = info.AudioStreams.First();

if (File.Exists(@"C:\video\video.wav"))
{
    File.Delete(@"C:\video\video.wav");
}
await FFmpeg.Conversions.New()
    .AddStream(audioStream)
    .SetOutputFormat(Format.wav)
    .SetOutput(@"C:\video\video.wav")
    .Start();

Console.WriteLine("DOne making images");


Thread.Sleep(400);


List<byte[]> bitmapList = new List<byte[]>();
string[] files = Directory.GetFiles("C:\\video\\Images", "*.png");

files.ToList().Sort();

foreach (string file in files)
{
    bitmapList.Add(File.ReadAllBytes(file));
}


string grayRamp = "$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/|()1{}[]?-_+~<>i!lI;:,\"^`'. ";

double rampLength = (double)grayRamp.Length;
Func<int, char> GetGrayScaleChar = (Func<int, char>)(grayScale => grayRamp[Convert.ToInt32(Math.Ceiling((rampLength - 1.0) * (double)grayScale / (double)byte.MaxValue))]);

Func<int, char> GetBlackOrWhiteChar = (Func<int, char>) (i => i > 128 ? '&' : ' ');


int boxX = 2;
int boxY = 2;
int LoopTimesX = videoStream.Width / boxX;
int LoopTimesY = videoStream.Height / boxY;

Task<Dictionary<int,string>>[] TaskArray = new Task<Dictionary<int, string>>[12];
Dictionary<int, byte[]>[] valuePairs = new Dictionary<int, byte[]>[TaskArray.Length];
for (int i = 0; i < bitmapList.Count; i++)
{
    for (int j = 0; j < valuePairs.Length; j++)
    {
        int index1 = i;
        int index2 = j;
        if (bitmapList.Count == i)
            { continue; }
        if (valuePairs[j] == null)
            valuePairs[j] = new Dictionary<int, byte[]>();
        valuePairs[index2].Add(index1, bitmapList[index1]);
        i++;
    }
}



for (int i = 0; i < TaskArray.Length; i++)
{
    int index = i;
    TaskArray[i] = Task.Run(() => MakeFrame(valuePairs[index], index));
    //Console.WriteLine(Marshal.SizeOf(valuePairs));
}

Console.ReadKey();

//valuePairs = null;
//bitmapList = null;
//files = null;

Dictionary<int, string> MakeFrame(Dictionary<int, byte[]> bitmapList, int threadNumber)
{
    Dictionary<int, string> frames = new Dictionary<int, string>();
    Span<char> row = stackalloc char[LoopTimesX];
    Span<char> wholeFrame = stackalloc char[(LoopTimesX * LoopTimesY) + LoopTimesX];
    int count = 0;
    foreach (KeyValuePair<int, byte[]> bitmaps in bitmapList)
    {
        using var image = Image.Load<Rgba32>(bitmaps.Value);

        string imageString = "";

        for (int i = 0; i < LoopTimesY; ++i)
        {

            for (int j = 0; j < LoopTimesX; ++j)
            {
                int[] colors = new int[boxX * boxY];
                for (int l = 0; l < boxX; ++l)
                {
                    for (int k = 0; k < boxY; ++k)
                    {
                        Rgba32 pixel = image[(boxX * j) + l, (boxY * i) + k];
                        byte r = pixel.R;
                        byte g = pixel.G;
                        byte b = pixel.B;
                        int int32 = Convert.ToInt32(0.21 * (double)r + 0.72 * (double)g + 0.07 * (double)b);
                        colors[l * k] = int32;
                    }
                }
                int average = 0;
                for (int index5 = 0; index5 < colors.Length; ++index5)
                    average += colors[index5];


                row[j] = GetGrayScaleChar(average / colors.Length);
            }
            for (int p = 0; p < row.Length; p++)
            {
                wholeFrame[i * (row.Length + 1) + p] = row[p]; // Place each row character in the wholeFrame
            }
            wholeFrame[i * (row.Length + 1) + row.Length] = '\n'; // Put a line break at the end of each row

        }

        Console.SetCursorPosition(0, threadNumber);
        Console.WriteLine("Frames made: " + count);
        count++;
        frames.Add(bitmaps.Key, wholeFrame.ToString());
    }

    


    return frames;
}

Task.WaitAll(TaskArray);
Console.WriteLine("All Task are done");
Console.ReadKey();
Console.Clear();
SortedDictionary<int, string> frames = new SortedDictionary<int, string>();
for (int i = 0; i < TaskArray.Length; i++)
{
    foreach (KeyValuePair<int, string> bitmaps in TaskArray[i].Result)
    {
        frames.Add(bitmaps.Key, bitmaps.Value);
    }
}

Console.WriteLine("Super done");
frames.OrderBy(x => x.Key);
Console.ReadKey();
Console.Clear();

DateTime _lastTime = DateTime.Now; // marks the beginning the measurement began
int _framesRendered = 0; // an increasing count
int _fps = 0; // the FPS calculated from the last measurement

Stopwatch stopwatch = new Stopwatch();
long lastupdate = 0;
stopwatch.Start();
SoundPlayer soundPlayer = new SoundPlayer();
soundPlayer.SoundLocation = @"C:\video\video.wav";
soundPlayer.Load();
soundPlayer.Play();

foreach (string frame in frames.Values)
{

    while (stopwatch.ElapsedTicks < lastupdate + (0.0333333333 * Stopwatch.Frequency))
    {
        Thread.Sleep(0); // I don't know C# so this is a guess, but there will be some equivalent function somewhere
    }
    lastupdate = stopwatch.ElapsedTicks;

    _framesRendered++;

    if ((DateTime.Now - _lastTime).TotalSeconds >= 1)
    {
        // one second has elapsed 

        _fps = _framesRendered;
        _framesRendered = 0;
        _lastTime = DateTime.Now;
    }

    Console.WriteLine(frame);
    Console.WriteLine("FPS: " + _fps);
    Console.WriteLine("Miliseconds: " + stopwatch.ElapsedMilliseconds);
    Console.WriteLine("Ticks: " + stopwatch.ElapsedTicks);
    Console.SetCursorPosition(0, 0);

}
soundPlayer.Stop();
stopwatch.Stop();
Console.WriteLine("DOne");
Console.ReadKey();


