# Matt.Encoding.Fountain
C# implementation of [Fountain Codes](https://en.wikipedia.org/wiki/Fountain_code)

![Build status](https://switchigan.visualstudio.com/_apis/public/build/definitions/9e65584e-ff3f-4616-b1ab-5227abae1502/10/badge "Build status")

## NuGet

[```Matt.Encoding.Fountain```](https://www.nuget.org/packages/Matt.Encoding.Fountain/)

## Overview

A [fountain code](https://en.wikipedia.org/wiki/Fountain_code) is a rateless forward error correction code. That means:
 * Data is split up into slices.
 * Only a subset of those slices are needed to get the data back.
 * There is no practical limit to how many distinct slices there can be.

## Practical applications

### Increase backup resiliency
 * Create 10 slices of your data, each holding 1/5 the size of your data.
 * Let any 4 of those be lost.
 * You'll probably still be able to get your data back!

### Increase users' download speeds
 * Make several of your servers serve slices from the same file.
   * No coordination between servers is required.
   * Just make sure you are not generating slices "systematically"--more on that below.
 * Let users download from all your servers at once to get slices extra fast.
 * After a user has collected enough slices, they'll be able to put them together and get your file!

## Examples

### Encoding slices

```csharp
// Start with some data
var data = new byte[]
{
  (byte)'C',
  (byte)'u',
  (byte)'t',
  (byte)'e',
  (byte)' ',
  (byte)'k',
  (byte)'i',
  (byte)'t',
  (byte)'t',
  (byte)'e',
  (byte)'n',
  (byte)'s',
};

// Slice up the data
var slices =
  SliceHelpers
    .CreateGenerator(
      data: data,
      sliceSize: 4,
      rngFactoryDelegate: () => new RandomAdapter(new Random()) // RandomAdapter is from https://www.nuget.org/packages/Matt.Random/
    )
    .Take(105) // How many slices do you want?
    .ToList();

// Put the slices somewhere
foreach (var slice in slices)
  await WriteToDiskAsync(slice);
```

### Decoding slices

```csharp
// Grab some slices
var random = new Random();
var slices =
  await ReadSlicesFromDiskAsync()
    .Where(_ => random.Next() % 2 == 0) // Oh no! We lost half the slices!
    .ToList();

// Set up a solver and let it know how large the original data is
var solver = new SliceSolver(data.Length);

// Tell the solver about the slices you've got
foreach (var slice in slices)
  await solver.RememberAsync(slice);

// Try solving for the original data
var solution = await solver.TrySolveAsync();
if (solution == null)
  throw new Exception("We need to gather more slices");

// Do something with the original data
await DoSomethingWithSolutionAsync(solution);
```

## Types

### `Slice`

A `Slice` is just an immutable data container that also describes from which subsections of original data it came.

#### `Slice.Coefficients : IReadOnlyList<bool>`

This read-only list of bits tells which subsections of the original data went into making this `Slice`.

#### `Slice.Data : IReadOnlyList<byte>`

This read-only list of bytes holds the combined subsections of original data.

### `SliceSolver`

Combines `Slice`s together to recover the original data.

#### `SliceSolver.RememberAsync(Slice slice) : Task`

Records the given `Slice` so that it can be used in the solution.

#### `SliceSolver.TrySolveAsync() : Task<byte[]>`

Asynchronously tries to solve for the original data. If `null` is returned then more `Slice`s need to be remembered.

### `SliceHelpers`

Static class with helper functions.

### `SliceHelpers.CreateGenerator(byte[] data, int sliceSize, Func<IRandom> rngFactoryDelegate, bool isSystematic = true) : IEnumerable<Slice>`

Parameters:

 * `data : byte[]` is the original data.
 * `sliceSize : int` is the size of each slice. This is the "1/_k_" that you'll see mentioned elsewhere in this readme.
 * `rngFactoryDelegate : Func<IRandom>` a delegate which returns a new instance of `IRandom` each time it's invoked. `IRandom` comes from [`Matt.Random`](https://www.nuget.org/packages/Matt.Random/)--there is an adapter in that NuGet package that lets you use a regular `Random` instance.
 * `isSystematic : bool` determines if the first _k_ `Slice`s that come out are the first _k_ subsections of the original data in order.

Returns an infinitely-long enumerable of `Slice`s. To generate another slice, just pull another element out of the returned enumerable.

## Technicalities

### This isn't magic

All that's happening behind the scenes is that random subsets of the original data are being XOR'd together for each slice. This is basically how all fountain codes work. The [Luby Transform](https://en.wikipedia.org/wiki/Luby_transform_code) fountain code just draws its randomness from a distribution with engineered properties. [Raptor Codes](https://en.wikipedia.org/wiki/Raptor_code) just pre-process the original data so that it's more likely that decoding will work sooner with fewer slices.

### Decoding is not deterministic

If you use a slice size of 1/_k_ (so that there are _k_ slices), you'll _probably_ be able to decode your original data after collecting _n_ slices. You have to have at least _k_ slices; after that your chances are governed by [the following equation](http://math.stackexchange.com/a/172112/284627):

![Probability of decoding n slices](https://raw.githubusercontent.com/matthew-a-thomas/Matt.Encoding.Fountain/master/img/Probability%20of%20decoding%20n%20slices.gif "Probability of decoding n slices")

Basically, by having _n_=_k_ slices you're guaranteed at least a 29% chance success (better if _k_ is small). After that the above equations says your chances will improve quickly. Here's a table showing the asymptotic limit of the above equation for various _n_ above _k_:

| _n_-_k_ | Your probability is at least: |
|---------|-------------------------------|
| 0 | 0.2887880950866024 |
| 1 | 0.5775761901732048 |
| 2 | 0.7701015868976065 |
| 3 | 0.8801160993115502 |
| 4 | 0.9387905059323203 |
| 5 | 0.9690740706398145 |
| 6 | 0.9844561987452083 |
| 7 | 0.9922078223573753 |
| 8 | 0.9960988334254435 |
| 9 | 0.9980481462110119 |
| 10 | 0.9990237553470930 |
| 15 | 0.9999694827323145 |
| 20 | 0.9999990463259868 |
| 25 | 0.9999999701976779 |
| 50 | 0.9999999999999991 |
| 53 | <0.9999999999999999 |
| 54 | >0.9999999999999999 |

Note that the number of nines after the decimal place follows this equation:

![Number of nines after decimal place](https://raw.githubusercontent.com/matthew-a-thomas/Matt.Encoding.Fountain/master/img/Number%20of%20nines%20after%20decimal%20place.gif "Number of nines after decimal place")

### There _is_ a limit to how many slices there can be

If you understand that each slice is just a random linear combination of subsections of your data, then you'll recognize that there can be this many distinct slices (where 1/_k_ is the size of each slice):

![Number of distinct slices](https://github.com/matthew-a-thomas/Matt.Encoding.Fountain/blob/master/img/Number%20of%20distinct%20slices.gif "Number of distinct slices")

But if you choose _k_ = 190 then you'll be able to generate more distinct slices than there are atoms in the Sun.
