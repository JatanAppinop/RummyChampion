using UnityEngine;

public static class ComponentExtensions
    {
        /// <summary>
        /// Extension method to get a gameobject's component, adding it if necessary.
        /// </summary>
        public static C GetOrAddComponent<C>( this MonoBehaviour script ) where C : Component
        {
            return script.gameObject.GetOrAddComponent<C>();
        }
 
        /// <summary>
        /// Extension method to get a gameobject's component, adding it if necessary.
        /// </summary>
        public static C GetOrAddComponent<C>( this GameObject gameObject ) where C : Component
        {
            C component = gameObject.GetComponent<C>();
            if ( component == null )
            {
                component = gameObject.AddComponent<C>();
            }
 
            return component;
        }
 
        /// <summary>
        /// Extension method to remove a gameobject's component.
        /// </summary>
        public static void RemoveComponent<C>( this MonoBehaviour script ) where C : Component
        {
            script.gameObject.RemoveComponent<C>();
        }
 
        /// <summary>
        /// Extension method to remove a gameobject's component.
        /// </summary>
        public static void RemoveComponent<C>( this GameObject gameObject ) where C : Component
        {
            C component = gameObject.GetComponent<C>();
            if ( component != null )
            {
                GameObject.DestroyObject(component);
            }
        }
 
        /// <summary>
        /// Extension method to remove all components of a given type C from a gameobject's hierarchy.
        /// </summary>
        public static void RemoveComponentsInChildren<C>( this MonoBehaviour script, bool includeInactive = true ) where C : Component
        {
            script.gameObject.RemoveComponentsInChildren<C>( includeInactive );
        }
 
        /// <summary>
        /// Extension method to remove all components of a given type C from a gameobject's hierarchy.
        /// </summary>
        public static void RemoveComponentsInChildren<C>( this GameObject gameObject, bool includeInactive = true  ) where C : Component
        {
            foreach ( C component in gameObject.GetComponentsInChildren<C>( includeInactive ) )
            {
                GameObject.DestroyObject(component);
            }
        }
}