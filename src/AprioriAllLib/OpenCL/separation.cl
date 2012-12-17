/// \addtogroup opencl
/// @{

/*!
	Puts 1 in supports array where the unique item exists.
	
	\param items array of all items of all transactions of input
	\param itemsCount length of items array
	\param supports output
 */
__kernel void separateUniqueItems(
	__global const int* items,
	__global const int* itemsCount,
	//__global const int* itemsTransactions,
	__global const int* uniqueItems,
	__global const int* uniqueItemsCount,
	__global int* supports
	)
{
	int x = get_global_id(0);
	int y = get_global_id(1);
	if(x < *itemsCount && y < *uniqueItemsCount)
	{
		int supportsIndex = y * *itemsCount + x;
		if(items[x] == uniqueItems[y])
			supports[supportsIndex] = 1;
		else
			supports[supportsIndex] = 0;
	}
}

/// @}
