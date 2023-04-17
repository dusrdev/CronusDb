# CronusDB

An incredibly performant key-value-pair based serializable/persistent in-memory database

## Features

* 🪶 Very Lightweight
* 🚀 Extremely fast read and write (Achieved single digit nano-second per operation in tests)
* 🔐 AES encryption baked in
* 📲 Completely cross platform
* 🔑 Apis almost identical to `Dictionary<TValue, TKey>`, no more complicated queries and Linq

## Where applications could benefit from CronusDB?

* Requirement for light persistent storage that could be encrypted, such as settings and preferences
* Managing users locally, encryption makes it easy to store and read users
* Local caching of data from requests or complex calculations
* Saving complex and custom data types natively and with encryption

## Where you should **NOT** use CronusDB?

* Applications where the database is large but memory is small, as the entire database is maintained in memory during usage
* Applications where you save large data, such as pictures, videos and so on
* Applications where you need want to store the data on a server, as the entire database is loaded to memory and re-written every time, the network usage could begin to add up very quickly, and the data on server and device could become de-synchronized

## Encryption

CronusDB contains 2 types of databases, they are similar at their core but each excels at different tasks.

Both an option to encrypt the entire database file

### Database type 1 - `Database`

This database is built around a `Dictionary<string, byte[]>`. It is extremely flexible as the native value type: `byte[]` could be used to represent pretty much everything.

This means that you can convert any type of data you like to a `byte[]` and save that inside the dictionary, which further means you can use different value types per different keys.

This flexibility is only increased by the fact that this database type supports **PER-KEY** encryption in addition to global database encryption.

This makes it perfect for saving user credentials and other things as even in memory the data in the encrypted key could not be read until you try with the correct key

### Database type 2 - `GenericDatabase`

This database is build around a `Dictionary<string, TValue>`. Here you must specify a single data type for the values, and upon initialization you must provide a `serialization` and `deserialization` functions to convert from `TValue` to `byte[]` and back.

Because of the guarantee of having the same type for the values, **PER-KEY** encryption is removed, but other options are added such as using `indexers` for accessing the values, and a `RemoveAll` functions that enables cache invalidation.

The `serialization` and `deserialization` functions are only used when reading or writing the database to the physical storage of the device. While the database is in-memory, the values are stored as `TValue` which enables rapid CRUD operations.

This type is perfect for caching calculations or responses from servers

## Credits for dependencies

* [MemoryPack](https://github.com/Cysharp/MemoryPack) For enabling best-in-class performance for reading and writing to physical storage 