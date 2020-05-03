using System;
using System.Collections.Generic;

using System.Threading;
#if WINDOWS_UWP
using System.Threading.Tasks;
#endif

namespace HoloFab {
	namespace CustomData {
		class ThreadInterface {
			public string sourceName = "Thread Intrface";
            
			#if WINDOWS_UWP
			// thread Object Reference.
			private CancellationTokenSource cancellation;
			private Task thread;
			#else
			// Thread Object Reference.
			private Thread thread = null;
			#endif
			// History:
			// - Debug History.
			public List<string> debugMessages = new List<string>();

			// Actual Action to be ran in the loop to be overridden.
			public Action threadAction;
            
			// Action Type to check if loop should break.
			public delegate bool LoopConditionCheck();
			// Actiual Action for loop checking to be overriden.
			public LoopConditionCheck checkCondition = CheckLoopCondition;
            
			// Default Check to run on Loop - infinite loop
			public static bool CheckLoopCondition() {
				return true;
			}
			// Infinite Loop Executing set function.
			public void ThreadLoop() {
				if (this.threadAction != null) {
					while (true) {// this.checkCondition()) {
						this.threadAction();
					}
				}
			}
            
			//////////////////////////////////////////////////////////////////////////
			#if WINDOWS_UWP
			public void Start() {
				if ((this.threadAction != null) && (this.thread == null)) {
					this.debugMessages = new List<string>();
					// Start the thread.
					this.cancellation = new CancellationTokenSource();
					this.thread = new Task(() => ThreadLoop(), this.cancellation.Token);
					this.thread.Start();
					#if DEBUG
					DebugUtilities.UniversalDebug(this.sourceName, "Thread Started.", ref this.debugMessages);
					#endif
				}
			}
			public void Stop() {
				// Reset.
				if (this.thread != null) {
					this.cancellation.Cancel();
					this.thread.Wait(1);
					this.cancellation.Dispose();
					this.thread = null;     // Good Practice?
					#if DEBUG
					DebugUtilities.UniversalDebug(this.sourceName, "Stopping Thread.", ref this.debugMessages);
					#endif
				}
			}
			#else
			public void Start() {
				if ((this.threadAction != null) && (this.thread == null)) {
					this.debugMessages = new List<string>();
					// Start the thread.
					this.thread = new Thread(new ThreadStart(ThreadLoop));
					this.thread.IsBackground = true;
					this.thread.Start();
					#if DEBUG
					DebugUtilities.UniversalDebug(this.sourceName, "Thread Started.", ref this.debugMessages);
					#endif
				}
			}
			public void Stop() {
				// Reset.
				if (this.thread != null) {
					this.thread.Abort();
					this.thread = null;
					#if DEBUG
					DebugUtilities.UniversalDebug(this.sourceName, "Stopping Thread.", ref this.debugMessages);
					#endif
				}
			}
			#endif
		}
	}
}