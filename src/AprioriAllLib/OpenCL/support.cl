/// \addtogroup opencl
/// @{

/*!
	Initializes supports array.
	
	\param items array of all items of all transactions of input
	\param itemsCount length of items array
 */
__kernel void supportInitial(
	__global const int* items,
	__global const int* itemsCount,
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

/*!
	Calculates support of single items, i.e.
	counts how many of each is stored if _different_ transactions.

	\param items array of all items of all transactions of input
	\param itemsCount length of items array
	\param itemsTransactions array with numbers from zero onward that indicates, 
		to which transaction a corresponding element of items array belongs
	\param step current distance between two compared items, a power of 2
 */
__kernel void supportDuplicatesRemoval(
	__global const int* items,
	__global const int* itemsCount,
	__global const int* itemsTransactions,
	__global const int* uniqueItems,
	__global const int* uniqueItemsCount,
	__global const int* step,
	__global int* supports
	)
{
	int x = get_global_id(0);
	int y = get_global_id(1);
	if(x < *itemsCount && y < *uniqueItemsCount && *step > 0 && x % (2 * *step) == 0)
	{
		int supportsIndex = y * *itemsCount + x;
		if(itemsTransactions[x] == itemsTransactions[x + *step] == uniqueItems[y]
			&& supports[supportsIndex + *step] == 1)
		{
			supports[supportsIndex] = 1;
			supports[supportsIndex + *step] = 0;
		}
		//if(itemsTransactions[x] != itemsTransactions[x + *step])
		//	supports[supportsIndex] = supports[supportsIndex];  + supports[supportsIndex + *step];
		//else if(itemsTransactions[x + *step] != itemsTransactions[x + (*step / 2)])
		//	supports[supportsIndex] = supports[supportsIndex] + supports[supportsIndex + *step];
		//else
		//	supports[supportsIndex] = fmax(supports[supportsIndex], supports[supportsIndex + *step]);
	}
}

//__kernel void supportSumUp(
//	__global const int* itemsCount,
//	__global const int* uniqueItems,
//	__global const int* uniqueItemsCount,
//	__global const int* step,
//	__global int* supports
//	)
//{
//	int x = get_global_id(0);
//	int y = get_global_id(1);
//	if(x < *itemsCount && y < *uniqueItemsCount && *step > 0 && x % (2 * *step) == 0)
//	{
//		int supportsIndex = y * *itemsCount + x;
//		supports[supportsIndex] = 1;
//		values[x] += values[x + *step];
//	}
//}

/// @}
