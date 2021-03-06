using System;
using UnityEngine;

namespace Pathfinding {
    /** Base for all path modifiers.
	 * \see MonoModifier
	 * Modifier */
    public interface IPathModifier
    {
        int Priority
        {
            get;
            set;
        }

        ModifierData input
        {
            get;
        }

        ModifierData output
        {
            get;
        }

        void ApplyOriginal(Path p);

        void Apply(Path p, ModifierData source);

        void PreProcess(Path p);
    }

    [Serializable]
    public abstract class PathModifier : IPathModifier
    {
        public int priority;

        [NonSerialized]
        public Seeker seeker;

        public abstract ModifierData input
        {
            get;
        }

        public abstract ModifierData output
        {
            get;
        }

        public int Priority
        {
            get
            {
                return this.priority;
            }
            set
            {
                this.priority = value;
            }
        }

        public void Awake(Seeker s)
        {
            this.seeker = s;
            if (s != null)
            {
                s.RegisterModifier(this);
            }
        }

        public void OnDestroy(Seeker s)
        {
            if (s != null)
            {
                s.DeregisterModifier(this);
            }
        }

        [Obsolete]
        public virtual void ApplyOriginal(Path p)
        {
        }

        public abstract void Apply(Path p, ModifierData source);

        [Obsolete]
        public virtual void PreProcess(Path p)
        {
        }
    }
}
