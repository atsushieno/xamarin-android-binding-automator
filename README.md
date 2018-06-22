
# Android Binding Hedges and Binding automator

## What is this?

It is a draft document for new binding ecosystem management service, which does NOT exist at all.

## How is this useful?

Xamarin.Android bindings have no solid ecosystem. In Android Java world, everything lives in Maven and they have solid chains of dependencies. Each component builds on specific versions of components.

NuGet packages basically do the same, but those Xamarin related components don't respect Java versions by themselves.  Also, Android bindings "should" be built for each Android package, but those bindings developers are often lazy and package bindings irrelevant to Java packages. That breaks strict dependency tracking.

Binding not iteratively built is another problem. Java libraries have strict dependencies and those Xamarin bindings should do the same, but they are not always built in timely manner.

## Hedges

Building bindings is, however, not a simple quick job. And there are many existing bindings that would bring in constraints on developing new bindings. However, general principles on the dependencies and building library ecosystem would apply to any library.

Therefore, in this library package service, bindings are represented as a "hedge". A hedge represents a set of dependency trees that don't have conflicts for each other. Conflicts occur wherever people have different set of bindings and binding types conflict. Anyone can build Java bindings for Android Jetpack components and there will be more than one bindings. Any Java library that depends on Jetpack could be bound too, but Binding developers would have to choose Binding A or Binding B. Those A and B are considered as conflict.

## To make "Parallel world shift" easier

Historically conflicts have been just suppressed. One is considered as "authoritative" and others were ignored. However people make mistake and those bindings become "wrong". They often have to keep "wrong" state because of API compatibility. Mono.Android.dll is one of such a cancer (big, big one). There are people who believe API must be kept forever. But for other people it is more important to keep up with the mainstream Android development.

We don't deny such compatibility-first people. But we support democracy. If true Android developers want the other way, it will become bigger and beat "wrong" people at market.

To achieve such a fair and "ready for best" system, it is important to easily provide "soft" migration to the right ecosystem. If "an ecosystem" follows the same rule to another ecosystem, then the migration will be easier (than nothing).

Ultimately, there will be completely new bindings to android.jar, which is Mono.Android.dll now. Mono.Android.dll is of a lot of technical debt and it should be scrapped at some stage. But political power against the idea is too strong and it's not easy to achieve. Unlike Xamarin.iOS which migrated from monotouch.dll to Xamarin.iOS.dll, there was no chance for Xamarin.Android to abandon garbage. Therefore the first step here would be to make "parallel world shift" easier and least painful.

## How Hedges work

A hedge is an entire set of entire binding trees. A tree represents a binding node, which "should" be consistent with Maven Java package dependency tree. Conformance to Java package dependency tree is not mandatory, but it will easily bring in conflicts and will be naturally eliminated.

There will be an initial hedge which contains existing Mono.Android.dll and AndroidSupportComponents.

On the other hand, there will be new set of slightly modified version of the same set of bindings but with Jetpack, as AndroidSupportComponents is out of date for it.

A hedge will be represented as a simple tree based data. Initially just as an XML or JSON. It has to be either a versioned store, and/or forks. One repository can contain multiple hedges, and they have identifiers.

Each components should be upgradable without Java package dependency changes (e.g. for updating metadata fixup). So the component version should be recorded different. The assembly version had better be independent of the Java library itself. Instead some kind of attribute for the library is desirable.

## Miscellaneous implementation principles

One of the biggest problem in AndroidSupportComponents is hard dependency on xcodebuild. The existing repository never builds without it, and it's a huge problem.

The new repository will NEVER depend on platform-specific tools such as make or xcodebuild for contributors. It will depend only on .NET toolchains such as MSBuild and optionally cross-platform portion of Powershell.

AndroidSupportComponents did not bring in the enumification effort in Mono.Android, so there are API glitches.

## Binding Automator

What makes it difficult to maintain a hedge is human resources. It is desirable that all the building and packaging steps are automated. It is just impossible to generate bindings without fixing API by metadata fixups (that's an unachievable goal). But once metadata fixup is done it should be almost automatically doable unless there are big API changes and/or additions.

Ideally, API diffs for target package updates should be automatically reported, and until the package maintainer marks as OK it would be distributed as "preview" "subject to API changes".

## automated Build and Package distribution

Bindings should be built and packaged no sooner the target Java library gets updated. In successful cases they just build, and brings in no regressions. Regression here means, unintended ABI breakage. Therefore ABI breakage should be reported as part of build. Such a package can keep shipped, but it should be marked as problematic. People can either build upon problematic libraries, or wait until problems gets resolved.

Resolution for API breakage is either 1) marked as intended, or 2) ABI gets fixed.


