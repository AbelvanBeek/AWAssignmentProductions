using Cloo;
using System;
using System.IO;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Template {

class Game
{
    // when GLInterop is set to true, the fractal is rendered directly to an OpenGL texture
    bool GLInterop = false;
    // load the OpenCL program; this creates the OpenCL context
    static OpenCLProgram ocl = new OpenCLProgram("../../program.cl");
    // find the kernel named 'device_function' in the program
    OpenCLKernel kernel = new OpenCLKernel(ocl, "device_function");
    OpenCLKernel clearKernel = new OpenCLKernel(ocl, "clear_function");
    // create an OpenGL texture to which OpenCL can send data
    OpenCLImage<int> image = new OpenCLImage<int>(ocl, 512, 512);
    OpenCLBuffer<uint> patternBuffer;
    OpenCLBuffer<uint> secondBuffer;

    // screen surface to draw to
    public Surface screen;
	// stopwatch
	Stopwatch timer;
	int generation = 0;
	// two buffers for the pattern: simulate reads 'second', writes to 'pattern'
	uint [] pattern;
	uint [] second;
	uint pw, ph; // note: pw is in uints; width in bits is 32 this value.
	// helper function for setting one bit in the pattern buffer
	void BitSet( uint x, uint y ) { pattern[y * pw + (x >> 5)] |= 1U << (int)(x & 31); }
	// helper function for getting one bit from the secondary pattern buffer
	uint GetBit( uint x, uint y ) { return (second[y * pw + (x >> 5)] >> (int)(x & 31)) & 1U; }
	// mouse handling: dragging functionality
	uint xoffset = 0, yoffset = 0;
	bool lastLButtonState = false;
	int dragXStart, dragYStart, offsetXStart, offsetYStart;
	public void SetMouseState( int x, int y, bool pressed )
	{
		if (pressed)
		{
			if (lastLButtonState)
			{
				int deltax = x - dragXStart, deltay = y - dragYStart;
				xoffset = (uint)Math.Min( pw * 32 - screen.width, Math.Max( 0, offsetXStart - deltax ) );
				yoffset = (uint)Math.Min( ph - screen.height, Math.Max( 0, offsetYStart - deltay ) );
			}
			else
			{
				dragXStart = x;
				dragYStart = y;
				offsetXStart = (int)xoffset;
				offsetYStart = (int)yoffset;
				lastLButtonState = true;
			}
		}
		else lastLButtonState = false;
	}
	// minimalistic .rle file reader for Golly files (see http://golly.sourceforge.net)
	public void Init()
	{
        timer = new Stopwatch();
        StreamReader sr = new StreamReader( "../../data/turing_js_r.rle" );
		uint state = 0, n = 0, x = 0, y = 0;
		while (true)
		{
			String line = sr.ReadLine();
			if (line == null) break; // end of file
			int pos = 0;
			if (line[pos] == '#') continue; /* comment line */ else if (line[pos] == 'x') // header
			{
				String[] sub = line.Split( new char[] { '=',',' }, StringSplitOptions.RemoveEmptyEntries );
				pw = (UInt32.Parse( sub[1] ) + 31) / 32;
				ph = UInt32.Parse( sub[3] );
                Console.WriteLine(pw + " " + ph);
				pattern = new uint[pw * ph];
				second = new uint[pw * ph];

			}
			else while (pos < line.Length)
			{
				Char c = line[pos++];
				if (state == 0) if (c < '0' || c > '9') { state = 1; n = Math.Max( n, 1 ); } else n = (uint)(n * 10 + (c - '0'));
				if (state == 1) // expect other character
				{
					if (c == '$') { y += n; x = 0; } // newline
					else if (c == 'o') for( int i = 0; i < n; i++ ) BitSet( x++, y ); else if (c == 'b') x += n;
					state = n = 0;
				}
			}
		}
		// swap buffers
		for( int i = 0; i < pw * ph; i++ ) second[i] = pattern[i];

        //    //Initialize buffers
        //    for (int i = 0; i < pw * ph; i++)
        //    {
        //        second[i] = 0;
        //        pattern[i] = 0;
        //    }
        //for(int i = 0; i < 10; i++)
        //    {
        //        second[i] = 1;
        //        pattern[i] = 1;
        //    }
        patternBuffer = new OpenCLBuffer<uint>(ocl, pattern);
        secondBuffer = new OpenCLBuffer<uint>(ocl, second);
        

	}
	// SIMULATE
	// Takes the pattern in array 'second', and applies the rules of Game of Life to produce the next state
	// in array 'pattern'. At the end, the result is copied back to 'second' for the next generation.
	void Simulate()
	{
		// clear destination pattern
		for( int i = 0; i < pw * ph; i++ ) pattern[i] = 0;
		// process all pixels, skipping one pixel boundary
		uint w = pw * 32, h = ph;
		for( uint y = 1; y < h - 1; y++ ) for( uint x = 1; x < w - 1; x++ )
		{
			// count active neighbors
			uint n = GetBit( x - 1, y - 1 ) + GetBit( x, y - 1 ) + GetBit( x + 1, y - 1 ) + GetBit( x - 1, y ) + 
					 GetBit( x + 1, y ) + GetBit( x - 1, y + 1 ) + GetBit( x, y + 1 ) + GetBit( x + 1, y + 1 );
			if ((GetBit( x, y ) == 1 && n ==2) || n == 3) BitSet( x, y );
		}
		// swap buffers
		for( int i = 0; i < pw * ph; i++ ) second[i] = pattern[i];
	}
	// TICK
	// Main application entry point: the template calls this function once per frame.
	public void Tick()
	{
		// start timer
		timer.Restart();
		// run the simulation, 1 step
		//Simulate();
        //patternBuffer = new OpenCLBuffer<uint>(ocl, pattern);
        //secondBuffer = new OpenCLBuffer<uint>(ocl, second);
            // visualize current state
            screen.Clear( 0 );
		//for( uint y = 0; y < screen.height; y++ ) for( uint x = 0; x < screen.width; x++ )
		//	if (GetBit( x + xoffset, y + yoffset ) == 1) screen.Plot( x, y, 0xffffff );
		// report performance
		Console.WriteLine( "generation " + generation++ + ": " + timer.ElapsedMilliseconds + "ms" );

        // do random OCL stuff
        GL.Finish();
        clearKernel.SetArgument(0, image);
        kernel.SetArgument(0, image);
        kernel.SetArgument(1, pw);
        kernel.SetArgument(2, ph);
        kernel.SetArgument(3, patternBuffer);
        kernel.SetArgument(4, secondBuffer);
        // execute kernel
        long[] workSize = { 512,512 };
        long[] localSize = { 32, 4 };
        kernel.LockOpenGLObject(image.texBuffer);
        clearKernel.Execute(workSize, null);
        kernel.Execute(workSize, localSize);
        kernel.UnlockOpenGLObject(image.texBuffer);

        }


        public void Render()
    {
        // use OpenGL to draw a quad using the texture that was filled by OpenCL
        GL.LoadIdentity();
        GL.BindTexture(TextureTarget.Texture2D, image.OpenGLTextureID);
        GL.Begin(PrimitiveType.Quads);
        GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-1.0f, -1.0f);
        GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(1.0f, -1.0f);
        GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(1.0f, 1.0f);
        GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-1.0f, 1.0f);
        GL.End();
    }
    }

} // namespace Template
