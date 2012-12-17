/// \addtogroup opencl
/// @{

/*!
	Counts distinct item values in a list, and creates an ID
	for each unique value.

	\param items array of all items of all transactions of input
	\param itemsCount length of items array
	\param itemsExcluded when 1, a corresponding element from items array 
		is already established as a distinct item
	\param step current distance between two compared items, a power of 2
 */
__kernel void findNewDistinctItem(
	__global const int* items,
	__global const int* itemsCount,
	__global const int* itemsExcluded,
	__global int* newItemsExcluded,
	//__global int* uniqueItems,
	//__global int* uniqueItemsCount,
	__global int* discovered,
	__global const int* step
	)
{
	int x = get_global_id(0);
	if(x < *itemsCount && discovered[x] == 0 && *step > 0 && x % (2 * *step) == 0)
	{
		if(itemsExcluded[x] == 0)
			discovered[x] = items[x];
		else if(itemsExcluded[x + *step] == 0)
		{
			discovered[x] = items[x + *step];
			newItemsExcluded[x] = 0;
		}
		else if(newItemsExcluded[x + *step] == 0)
		{
			discovered[x] = discovered[x + *step];
		}
	}
}

/*!
	Excludes latest discovered distinct value from items.

	\param items array of all items of all transactions of input
	\param itemsCount length of items array
	\param itemsExcluded when 1, a corresponding element from items array 
		is already present in uniqueItems array
	\param step current step number
 */
__kernel void excludeLatestDistinctItem(
	__global const int* items,
	__global const int* itemsCount,
	__global int* itemsExcluded,
	__global const int* uniqueItems,
	__global const int* uniqueItemsCount
	)
{
	int x = get_global_id(0);
	if(x < *itemsCount)
	{
		if(*uniqueItemsCount == 0)
			itemsExcluded[x] = 0;
		else if(items[x] == uniqueItems[*uniqueItemsCount - 1])
			itemsExcluded[x] = 1;
	}
}

/// @}
