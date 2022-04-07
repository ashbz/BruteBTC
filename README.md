
# BruteBTC

![](https://i.imgur.com/RKJkkWN.png)

### Description

BruteBTC is a BTC wallet collider developed with C#. It can check over 30000 wallets a second with a modern CPU. 
Should work on Windows, Mac OS and Linux, although it has been compiled and tested only on Windows.

##### What does a BTC key collider do?
A BTC wallet collider generates random private keys and checks if those private keys have the same public address as one of the known wallets with balance in them (if there is a collision).

##### How likely is it that such a collision will happen?
According to [Bitcoin Wiki](https://en.bitcoin.it/wiki/Technical_background_of_version_1_Bitcoin_addresses "Bitcoin Wiki"):
> Because the space of possible addresses is so astronomically large it is more likely that the Earth is destroyed in the next 5 seconds, than that a collision occur in the next millenium.

So chances are pretty slim, but you just never know.

## How to use
1. Download a list of the known BTC wallets with balance in them from here:  http://addresses.loyce.club/
2. Extract the .tsv file
3. Drag and drop the .tsv file into BruteBTC.exe

The program will then read the wallet addresses into memory, and then once that is done it will start generating private keys and checking.

## Requirements
- At least 8GB of free RAM (loading all wallets into memory will take 7GB)
- A good CPU with multiple cores (program uses multi-threading for faster checking)
- .NET Core 3.1

## Uses
- [NBitcoin](https://github.com/MetacoSA/NBitcoin "NBitcoin")
