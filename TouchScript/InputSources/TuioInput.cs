﻿/*
 * Copyright (C) 2012 Interactive Lab
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
 * 
 */

using System.Collections.Generic;
using TUIOsharp;
using UnityEngine;

namespace TouchScript.InputSources {
    /// <summary>
    /// Processes TUIO 1.0 input.
    /// </summary>
    [AddComponentMenu("TouchScript/Input Sources/TUIO Input")]
    public class TuioInput : InputSource {
        #region Unity fields

        /// <summary>
        /// Port to listen to.
        /// </summary>
        public int TuioPort = 3333;

        /// <summary>
        /// Use touch flicker prevention.
        /// Handles touches disappearing and reappearing again in short period of time.
        /// </summary>
        //public bool PreventTouchFlicker = true;
        /// <summary>
        /// Maximum distance in cm for a new touch to be considered as disappeared old touch.
        /// </summary>
        //public float TouchFlickerDistance = 0.5f;
        /// <summary>
        /// Maximum time in seconds while touch is considered to be still alive.
        /// </summary>
        //public float TouchFlickerDelay = 0.03f;

        #endregion

        #region Private variables
        private TuioServer server;
        private Dictionary<TuioCursor, int> cursorToInternalId = new Dictionary<TuioCursor, int>();

        #endregion

        #region Unity

        protected override void Start() {
            base.Start();

            server = new TuioServer();
            server.CursorAdded += OnCursorAdded;
            server.CursorUpdated += OnCursorUpdated;
            server.CursorRemoved += OnCursorRemoved;
            server.Connect();
        }

        protected override void Update() {
            base.Update();
        }

        protected override void OnDestroy() {
            if (server != null) {
                server.CursorAdded -= OnCursorAdded;
                server.CursorUpdated -= OnCursorUpdated;
                server.CursorRemoved -= OnCursorRemoved;
                server.Disconnect();
            }
            base.OnDestroy();
        }

        #endregion

        #region Private functions

        #endregion

        #region Event handlers

        private void OnCursorAdded(object sender, TuioCursorEventArgs tuioCursorEventArgs) {
            var cursor = tuioCursorEventArgs.Cursor;
            lock (this) {
                var x = cursor.X*ScreenWidth;
                var y = (1 - cursor.Y)*ScreenHeight;
                cursorToInternalId.Add(cursor, beginTouch(new Vector2(x, y)));
            }
        }

        private void OnCursorUpdated(object sender, TuioCursorEventArgs tuioCursorEventArgs) {
            var cursor = tuioCursorEventArgs.Cursor;
            lock (this) {
                int existingCursor;
                if (!cursorToInternalId.TryGetValue(cursor, out existingCursor)) return;

                var x = cursor.X*ScreenWidth;
                var y = (1 - cursor.Y)*ScreenHeight;

                moveTouch(existingCursor, new Vector2(x, y));
            }
        }

        private void OnCursorRemoved(object sender, TuioCursorEventArgs tuioCursorEventArgs) {
            var cursor = tuioCursorEventArgs.Cursor;
            lock (this) {
                int existingCursor;
                if (!cursorToInternalId.TryGetValue(cursor, out existingCursor)) return;

                cursorToInternalId.Remove(cursor);
                endTouch(existingCursor);
            }
        }

        #endregion
    }
}