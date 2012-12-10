__kernel void countSubsetsSupport(
		__global const int* setsCount,
		__global const int* sets, 
		__global const int* setSizes, 
		__global const int* subsetSize, 
		__global int* supports)
{
	unsigned int index = get_global_id(0) * get_global_size(0) + get_local_id(0);
	//if(index >= *setsCount)
	//	return;
	supports[index] = 3;
}
