using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Cloo;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Threading;

namespace Template {

class Game
{
	// load the OpenCL program; this creates the OpenCL context
	static OpenCLProgram ocl = new OpenCLProgram( "../../program.cl" );
	// find the kernel named 'device_function' in the program
	OpenCLKernel kernel = new OpenCLKernel( ocl, "show_function" );
    // find the kernel named 'device_function' in the program
    OpenCLKernel clearKernel = new OpenCLKernel(ocl, "clear_function");
    OpenCLKernel simulateKernel = new OpenCLKernel(ocl, "simulate_function");
    OpenCLKernel copyKernel = new OpenCLKernel(ocl, "copy_function");
    // create an OpenGL texture to which OpenCL can send data
    OpenCLImage<int> image = new OpenCLImage<int>( ocl, 512, 512 );
    OpenCLBuffer<uint> secondBuffer;
    OpenCLBuffer<uint> patternBuffer;

    public Surface screen;
	Stopwatch timer = new Stopwatch();
    int generation = 0;

    // two buffers for the pattern: simulate reads 'second', writes to 'pattern'
    uint[] pattern;
    uint[] second;
    uint pw, ph; // note: pw is in uints; width in bits is 32 this value.
    uint xoffset = 0, yoffset = 0;
    // helper function for setting one bit in the pattern buffer
    void BitSet(uint x, uint y) { pattern[y * pw + (x >> 5)] |= 1U << (int)(x & 31); }
    bool lastLButtonState = false;
    int dragXStart, dragYStart, offsetXStart, offsetYStart;
    public void SetMouseState(int x, int y, bool pressed)
    {
        if (pressed)
        {
            if (lastLButtonState)
            {
                int deltax = x - dragXStart, deltay = y - dragYStart;
                xoffset = (uint)Math.Min(pw * 32 - screen.width, Math.Max(0, offsetXStart - deltax));
                yoffset = (uint)Math.Min(ph - screen.height, Math.Max(0, offsetYStart - deltay));
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

    public void Init()
	{
        StreamReader sr = new StreamReader("../../data/turing_js_r.rle");
        uint state = 0, n = 0, x = 0, y = 0;
        while (true)
        {
            String line = sr.ReadLine();
            if (line == null) break; // end of file
            int pos = 0;
            if (line[pos] == '#') continue; /* comment line */
            else if (line[pos] == 'x') // header
            {
                String[] sub = line.Split(new char[] { '=', ',' }, StringSplitOptions.RemoveEmptyEntries);
                pw = (UInt32.Parse(sub[1]) + 31) / 32;
                ph = UInt32.Parse(sub[3]);
                pattern = new uint[pw * ph];
                second = new uint[pw * ph];
            }
            else while (pos < line.Length)
                {
                    Char c = line[pos++];
                    if (state == 0) if (c < '0' || c > '9') { state = 1; n = Math.Max(n, 1); } else n = (uint)(n * 10 + (c - '0'));
                    if (state == 1) // expect other character
                    {
                        if (c == '$') { y += n; x = 0; } // newline
                        else if (c == 'o') for (int i = 0; i < n; i++) BitSet(x++, y); else if (c == 'b') x += n;
                        state = n = 0;
                    }
                }
        }
            // swap buffers
        for (int i = 0; i < pw * ph; i++) { second[i] = pattern[i]; }

        secondBuffer = new OpenCLBuffer<uint>(ocl, second);
        patternBuffer = new OpenCLBuffer<uint>(ocl, pattern);
    }

	public void Tick()
	{ 
        timer.Restart();
        GL.Finish();
		// clear the screen
		//screen.Clear( 0 );
        // do opencl stuff
        clearKernel.SetArgument( 0, image );

        simulateKernel.SetArgument(0, secondBuffer );
        simulateKernel.SetArgument(1, patternBuffer);
        simulateKernel.SetArgument(2, pw);
        simulateKernel.SetArgument(3, ph);

        copyKernel.SetArgument(0, secondBuffer);
        copyKernel.SetArgument(1, patternBuffer);

        kernel.SetArgument( 0, image );
        kernel.SetArgument( 1, pw );
        kernel.SetArgument( 2, xoffset );
        kernel.SetArgument( 3, yoffset );
        kernel.SetArgument( 4, secondBuffer );
		kernel.SetArgument( 5, patternBuffer );

        // execute kernel
		long [] workSize = { 512, 512 };
		long [] localSize = { 32, 4 };
        long [] arraySizeExpanded = { pw * 32, ph }; //54 * 32 * 1647
        long [] arraySizeCompressed = { pattern.Length }; //54 * 1647

        // INTEROP PATH:
        // lock the OpenGL texture for use by OpenCL
        kernel.LockOpenGLObject( image.texBuffer );

        // execute the kernels
        clearKernel.Execute(workSize, localSize);
        simulateKernel.Execute(arraySizeExpanded, null);
        copyKernel.Execute(arraySizeCompressed, null);
        kernel.Execute( workSize, localSize );
            //Thread.Sleep(200);

		// unlock the OpenGL texture so it can be used for drawing a quad
		kernel.UnlockOpenGLObject( image.texBuffer );
        Console.WriteLine("generation " + generation++ + ": " + timer.ElapsedMilliseconds + "ms");
        
    }
	public void Render() 
	{
		// use OpenGL to draw a quad using the texture that was filled by OpenCL
		GL.LoadIdentity();
		GL.BindTexture( TextureTarget.Texture2D, image.OpenGLTextureID );
		GL.Begin( PrimitiveType.Quads );
		GL.TexCoord2( 0.0f, 1.0f ); GL.Vertex2( -1.0f, -1.0f );
		GL.TexCoord2( 1.0f, 1.0f ); GL.Vertex2(  1.0f, -1.0f );
		GL.TexCoord2( 1.0f, 0.0f ); GL.Vertex2(  1.0f,  1.0f );
		GL.TexCoord2( 0.0f, 0.0f ); GL.Vertex2( -1.0f,  1.0f );
		GL.End();
	}
}

} // namespace Template