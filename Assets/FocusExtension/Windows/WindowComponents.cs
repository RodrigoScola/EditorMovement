using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Focus
{
    public class Components
    {
        public static Transform NextComponent(Transform component)
        {
            var parent = component.transform.parent;

            var ind = component.GetSiblingIndex();

            if (parent is null)
            {
                var root = GetRootObjects();
                ind = Array.IndexOf(root, component);

                try
                {
                    return root[ind - 1];
                }
                catch (Exception)
                {
                    return null;
                }
            }

            try
            {
                return parent.GetChild(ind - 1);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Transform PreviousComponent(Transform component)
        {
            var parent = component.transform.parent;

            var ind = component.GetSiblingIndex();

            if (parent is not null)
            {
                try
                {
                    return parent.GetChild(ind + 1);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            var root = GetRootObjects();
            ind = Array.IndexOf(root, component);

            try
            {
                return root[ind + 1];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Transform[] GetRootObjects()
        {
            return UnityEngine
                .SceneManagement.SceneManager.GetActiveScene()
                .GetRootGameObjects()
                .Select(go => go.transform)
                .ToArray();
        }
    }
}
