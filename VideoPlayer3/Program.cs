


#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Xabe.FFmpeg;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using VideoPlayer3;


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
IMediaInfo info = await FFmpeg.GetMediaInfo(@"C:\video\video123.mp4").ConfigureAwait(false);
IVideoStream videoStream = info.VideoStreams.First()?.SetCodec(VideoCodec.png);

IConversionResult conversionResult = await FFmpeg.Conversions.New()
    .AddStream(videoStream)
    .ExtractEveryNthFrame(1, outputFileNameBuilder)
    .Start();

Console.WriteLine("DOne");


Thread.Sleep(400);


List<Bitmap> bitmapList = new List<Bitmap>();
string[] files = Directory.GetFiles("C:\\video\\Images", "*.png");

files.ToList().Sort();

foreach (string file in files)
{
    bitmapList.Add(new Bitmap(Image.FromFile(file)));
}


string grayRamp = "@%#*=-:. ";

double rampLength = (double)grayRamp.Length;
Func<int, char> GetGrayScaleChar = (Func<int, char>)(grayScale => grayRamp[Convert.ToInt32(Math.Ceiling((rampLength - 1.0) * (double)grayScale / (double)byte.MaxValue))]);

Func<int, char> GetBlackOrWhiteChar = (Func<int, char>) (i => i > 128 ? '&' : ' ');

List<string> frames = new List<string>();


int boxX = 1;
int boxY = 1;
int LoopTimesX = bitmapList[0].Width / boxX;
int LoopTimesY = bitmapList[0].Height / boxY;

Console.Clear();
foreach (Bitmap bitmap in bitmapList)
{
    string image = "";
    
    for (int i = 0; i < LoopTimesY; ++i)
    {
        for (int j = 0; j < LoopTimesX; ++j)
        {
            int[] colors = new int[boxX * boxY];
            for (int l = 0; l < boxX; ++l)
            {
                for (int k = 0; k < boxY; ++k)
                {
                    Color pixel = bitmap.GetPixel((boxX * j)  + l, (boxY * i) + k);
                    byte r = pixel.R;
                    pixel = bitmap.GetPixel((boxX * j)  + l, (boxY * i) + k);
                    byte g = pixel.G;
                    pixel = bitmap.GetPixel((boxX * j)  + l, (boxY * i) + k);
                    byte b = pixel.B;
                    int int32 = Convert.ToInt32(0.21 * (double)r + 0.72 * (double)g + 0.07 * (double)b);
                    colors[l * k] = int32;
                }
            }
            int average = 0;
            for (int index5 = 0; index5 < colors.Length; ++index5)
                average += colors[index5];
        
            image += GetGrayScaleChar(average / colors.Length).ToString() + "";
        }
        image += "\n";
    }
    
    frames.Add(image);
    Console.SetCursorPosition(0,0);
    Console.WriteLine("Frames made: " + frames.Count);
}


Console.WriteLine("Super done");
Console.ReadKey();
Console.Clear();


for (int i = 0; i < 10; i++)
{
    foreach (string frame in frames)
    {
        Thread.Sleep(16);
        Console.WriteLine(frame);
        Console.SetCursorPosition(0,0);
    }
}

Console.WriteLine("DOne");
Console.ReadKey();


