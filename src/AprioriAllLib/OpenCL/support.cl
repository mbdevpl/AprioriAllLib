/// \addtogroup opencl
/// @{

/*!
	Calculates support of single items, i.e.
	counts how many of each is stored if _different_ transactions.
 */
__kernel void calculateSupport(
		__global const int* itemList,
		__global const int* itemListLength, 
		__global const int* itemListLimits, 
		__global const int* itemLitsLimitsCount, 
		__global int* supports
		)
{
	unsigned int index = get_global_id(0); // * get_global_size(0) + get_local_id(0);
	//if(index >= *itemList)
	//	return;
	//supports[index] = index;
}

/// @}
