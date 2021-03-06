﻿// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;

namespace YAXL.Redux
{
    // All actions are actually objects
    using ActionType = System.Object;

    public delegate ActionType Dispatcher(ActionType action);
    public delegate T Reducer<T>(T state, ActionType action);

    public delegate Store<T> StoreCreator<T>(Reducer<T> reducer, T initialState, Enhancer<T> enhancer);
    public delegate StoreCreator<T> Enhancer<T>(StoreCreator<T> createStore);

    public partial class Store<T>
    {
        bool isDispatching;
        Reducer<T> reducer;

        public T State { get; private set; }
        public Dispatcher Dispatch { get; private set; }

        public delegate void Subscriber(T state);
        public event Subscriber Subscribe;

        public Store(Reducer<T> reducer, T initialState = default(T))
        {
            this.reducer = reducer;
            State = initialState;
            Dispatch = DoDispatch;
        }

        ActionType DoDispatch(ActionType action)
        {
            if (isDispatching)
                throw new InvalidOperationException("Cannot dispatch while dispatching");

            try
            {
                isDispatching = true;
                State = reducer(State, action);
            }
            finally
            {
                isDispatching = false;
            }

            Subscribe?.Invoke(State);

            return action;
        }

        public Store<T> ReplaceDispatch(Dispatcher dispatch)
        {
            Dispatch = dispatch;
            return this;
        }

        public static Store<T> CreateStore(Reducer<T> reducer, T initialState, Enhancer<T> enhancer = null)
        {
            if (enhancer != null)
                return enhancer(CreateStore)(reducer, initialState, null);

            return new Store<T>(reducer, initialState);
        }
    }
}
