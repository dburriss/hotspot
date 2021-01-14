---
title: Limitations
category: explanation
menu_order: 2
---

# Limitations

In version 0 there are currently some limitations to be aware of.

## Comments when using built-in LoC

Currently, when not using SCC, Hotspot will using lines of code (LoC) as a measure of complexity of a file to make recommendations on. The default LoC counter assumes a comment is `//`. Obviously this is extremely naive and will be improved in the future.

## Git bin location

The git binary is used under the hood. If the application is complaining about not finding it, set env variable 'HOTSPOT_GIT_EXECUTABLE' with the path to the exe/binary.

## Colour on console

The colours cannot be turned off on the console output. This will be changed in the future.