// helper function for setting one bit in the pattern buffer
void BitSet( uint x, uint y, uint pw, global uint *pattern ) { atomic_or(&pattern[y * pw + (x >> 5)], 1U << (int)(x & 31)); }

// helper function for getting one bit from the secondary pattern buffer
uint GetBit( uint x, uint y, uint pw, global uint *second ) { return (second[y * pw + (x >> 5)] >> (int)(x & 31)) & 1U; }

__kernel void simulate_function( global uint *second, global uint *pattern, uint pw, uint ph )
{
	int idx = get_global_id( 0 );
	int idy = get_global_id( 1 );
	int id = idx + ph * idy;

	int x = id % ph;
	int y = id / ph;

	if (x < 1 || y < 1) return;
	// count active neighbors
	uint n = GetBit( x - 1, y - 1, pw, second ) + GetBit( x, y - 1, pw, second ) + GetBit( x + 1, y - 1, pw, second ) + GetBit( x - 1, y, pw, second ) + 
				GetBit( x + 1, y, pw, second ) + GetBit( x - 1, y + 1, pw, second ) + GetBit( x, y + 1, pw, second ) + GetBit( x + 1, y + 1, pw, second );
	if ((GetBit( x, y, pw, second ) == 1 && n == 2) || n == 3) BitSet( x, y, pw, pattern );
}

__kernel void copy_function( global uint *second, global uint *pattern, uint pw )
{	
	int idx = get_global_id( 0 );
	int idy = get_global_id( 1 );
	int id = idx + pw * idy;
	second[id] = pattern[id];
	pattern[id] = 0;
}

__kernel void show_function( write_only image2d_t a, uint pw, uint xoffset, uint yoffset, global uint *second, global uint *pattern, float zoom )
{
	int idx = get_global_id( 0 );
	int idy = get_global_id( 1 );
	int id = idx + 512 * idy;

	int x = id % 512;
	int y = id / 512;
	write_imagef( a, (int2)(x, y), GetBit((uint)((float)(x + xoffset) / zoom),(uint)((float)(y + yoffset) / zoom), pw, second) );

}

__kernel void clear_function(
	write_only image2d_t a)
{
	// get the thread id; this will be our pixel id
	int idx = get_global_id( 0 );
	int idy = get_global_id( 1 );
	int id = idx + 512 * idy;

	int x = id % 512;
	int y = id / 512;
	write_imagef( a, (int2)(x, y), 0 );
}
