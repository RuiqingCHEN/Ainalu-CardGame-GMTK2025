using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSystem : Singleton<ActionSystem>
{
    private List<GameAction> reactions = null;
    public bool IsPerforming { get; private set; } = false;
    private static Dictionary<Type, List<Action<GameAction>>> preSubs = new();
    private static Dictionary<Type, List<Action<GameAction>>> postSubs = new();
    private static Dictionary<Type, Func<GameAction, IEnumerator>> performers = new();
    
    private static Dictionary<object, Dictionary<Type, Action<GameAction>>> delegateMap = new();

    public void Perform(GameAction action, System.Action OnPerformFinished = null)
    {
        if(IsPerforming) return;
        IsPerforming = true;
        StartCoroutine(Flow(action, () =>
        {
            IsPerforming = false;
            OnPerformFinished?.Invoke();
        }));
    }

    public void AddReaction(GameAction gameAction)
    {
        reactions?.Add(gameAction);
    }

    private IEnumerator Flow(GameAction action, Action OnFlowFinished = null)
    {
        reactions = action.PreReactions;
        PerformSubscribers(action, preSubs);
        yield return PerformReactions();

        reactions = action.PerformReactions;
        yield return PerformPerformer(action);
        yield return PerformReactions();

        reactions = action.PostReactions;
        PerformSubscribers(action, postSubs);
        yield return PerformReactions();

        OnFlowFinished?.Invoke();
    }

    private IEnumerator PerformReactions()
    {
        foreach (var reaction in reactions)
        {
            yield return Flow(reaction);
        }
    }

    private IEnumerator PerformPerformer(GameAction action)
    {
        Type type = action.GetType();
        if(performers.ContainsKey(type))
        {
            yield return performers[type](action);
        }
    }

    private void PerformSubscribers(GameAction action, Dictionary<Type, List<Action<GameAction>>> subs)
    {
        Type type = action.GetType();
        if (subs.ContainsKey(type))
        {
            var subsCopy = new List<Action<GameAction>>(subs[type]);
            foreach (var sub in subsCopy)
            {
                try
                {
                    sub(action);
                }
                catch (MissingReferenceException)
                {
                    subs[type].Remove(sub);
                }
            }
        }
    }

    public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction
    {
        Type type = typeof(T);
        IEnumerator wrappedPerformer(GameAction action) => performer((T)action);
        if (performers.ContainsKey(type)) performers[type] = wrappedPerformer;
        else performers.Add(type, wrappedPerformer);
    }
    
    public static void DetachPerformer<T>() where T : GameAction
    {
        Type type = typeof(T);
        if (performers.ContainsKey(type)) performers.Remove(type);
    }
    
    public static void SubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        Dictionary<Type, List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        void wrappedReaction(GameAction action) => reaction((T)action);
        
        Type actionType = typeof(T);
        
        // 存储委托映射关系
        var target = reaction.Target;
        if (target != null)
        {
            if (!delegateMap.ContainsKey(target))
                delegateMap[target] = new Dictionary<Type, Action<GameAction>>();
            delegateMap[target][actionType] = wrappedReaction;
        }
        
        if (subs.ContainsKey(actionType))
        {
            subs[actionType].Add(wrappedReaction);
        }
        else
        {
            subs.Add(actionType, new List<Action<GameAction>>());
            subs[actionType].Add(wrappedReaction);
        }
    }
    
    public static void UnsubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        Dictionary<Type, List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        Type actionType = typeof(T);
        
        var target = reaction.Target;
        if (target != null && delegateMap.ContainsKey(target) && delegateMap[target].ContainsKey(actionType))
        {
            var wrappedReaction = delegateMap[target][actionType];
            if (subs.ContainsKey(actionType))
            {
                subs[actionType].Remove(wrappedReaction);
                if (subs[actionType].Count == 0)
                {
                    subs.Remove(actionType);
                }
            }
            
            // 清理映射
            delegateMap[target].Remove(actionType);
            if (delegateMap[target].Count == 0)
            {
                delegateMap.Remove(target);
            }
        }
    }
    public static void ClearAllSubscriptions()
    {
        preSubs.Clear();
        postSubs.Clear();
        performers.Clear();
        delegateMap.Clear();
    }
}