# Project Specification: ScreenShield

## Goal
Build a privacy tool for open offices that automatically masks specific monitors when the user is inactive or triggers a shortcut.

## MVP Features (Phase 1)
1. **Monitor Detection:** Detect all active screens using `System.Windows.Forms.Screen` (or equivalent System.Drawing logic compatible with WPF).
2. **The Overlay:** A transparent WPF window capable of rendering "TopMost" over other apps.
3. **Trigger Logic:** A basic Global Mouse Hook (LowLevelMouseProc) that detects activity.

## Security Requirements
- The Global Hook must be resilient. If the app crashes, the hook must detach.
- The app must run with the least privilege necessary, though UI automation often requires higher trust.