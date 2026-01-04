Title: Why Decimal Numbers Break in My Web Soroban
Author: david-marin-0xff

Summary
-------
While building a Web-based Soroban (Japanese abacus), I noticed that
numbers after the decimal point sometimes become distorted, even
though the beads clearly represent an exact decimal value.

Example:
Expected:  10000000000.00101
Displayed: 10000000000.0010108948

This document explains why this happens and what I learned from it.


1. The Soroban Is an Exact Base-10 Machine
-----------------------------------------
A Soroban represents numbers in pure base-10:

- Each vertical rod is a power of 10
- Left of the decimal line: 10¹, 10², 10³, ...
- Right of the decimal line: 10⁻¹, 10⁻², 10⁻³, ...

Every bead has an exact meaning.
For example:
- One bead on the first rod right of the decimal = 0.1
- One bead two rods right of the decimal = 0.01

There is no rounding or approximation.
The Soroban is mathematically exact.


2. Computers Do NOT Use Base-10 Internally
------------------------------------------
JavaScript (and most programming languages) store numbers using
binary floating-point (IEEE-754).

Binary floating-point:
- Works in base-2, not base-10
- Can exactly represent values like:
  - 0.5 (1/2)
  - 0.25 (1/4)
- Cannot exactly represent values like:
  - 0.1
  - 0.01
  - 0.001

So when a program stores a decimal number, it often stores
the closest binary approximation instead of the exact value.


3. What Goes Wrong in the First Example
---------------------------------------
Example displayed by the Soroban:

60000050000.0006

Internally, the computer stores something closer to:

60000050000.00060000000000002722...

When the value is printed or recalculated, the hidden approximation
becomes visible, causing extra digits to appear.


4. Why the Second Example Looks Even Worse
------------------------------------------
In the second screenshot, the fractional part is more complex:

Expected:
10000000000.00101

Displayed:
10000000000.0010108948

This happens because:
- Each fractional rod adds a value like 10⁻n
- Each addition introduces a small binary rounding error
- Errors accumulate as more decimal rods are used

The integer part remains perfect.
The decimal part slowly drifts.


5. Important Realization
------------------------
This is NOT a bug in:
- The Soroban logic
- The bead math
- The drawing code

This is a fundamental limitation of floating-point arithmetic.

The Soroban is correct.
The computer is approximating.


6. How Real Software Solves This
-------------------------------
Serious software avoids floating-point for decimals:

- Financial systems
- Accounting software
- Precise math tools

Common solutions:
1) Store numbers as strings (digit by digit)
2) Store scaled integers (value × 10ⁿ)
3) Use decimal / big-number types instead of floating-point


7. What I Learned
-----------------
Building a Soroban visually exposed a classic computer-science problem:

Computers lie about decimals.

The Soroban doesn’t.

This project accidentally became a lesson in:
- Numeric representation
- Floating-point precision
- Why base-10 math is hard for binary machines


Conclusion
----------
If you want exact decimal behavior, you must avoid floating-point math.

The Soroban is a perfect reminder that:
representation matters more than calculation.
