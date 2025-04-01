using System;
using System.Collections.Generic;
using Godot;

namespace Core
{
    /// <summary>
    /// Base class for all drawable shapes in the application.
    /// Implements IDisposable for proper resource management.
    /// </summary>
    [GlobalClass]
    public partial class BentukDasar : Node2D, IDisposable
    {
        private bool _disposed = false;

        /// <summary>
        /// Releases all resources used by the BentukDasar object.
        /// </summary>
        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the BentukDasar and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected new void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Free managed resources here
                    base.Dispose();
                }

                // Free unmanaged resources here

                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer to ensure resources are cleaned up if Dispose is not called.
        /// </summary>
        ~BentukDasar()
        {
            Dispose(false);
        }
    }
}
