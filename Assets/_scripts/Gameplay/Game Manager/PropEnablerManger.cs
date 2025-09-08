using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropEnablerManger : MonoBehaviour
{
    
    [Serializable]
    public class Prop
    {
        public string propName;
        public int id;
        public GameObject prop;
    }
    
    public List<Prop> props = new List<Prop>();
    private readonly Dictionary<int, Prop> _byId = new Dictionary<int, Prop>();


    private void OnEnable()
    {
        GameManager.OnPropEnable += EnablePropById;
    }
    private void OnDisable()
    {
        GameManager.OnPropEnable -= EnablePropById;
    }
    
    void Awake()
    {
        _byId.Clear();
        foreach (var prop in props)
        {
            if (prop == null || prop.prop == null) continue;
            if (!_byId.ContainsKey(prop.id))
                _byId.Add(prop.id, prop);
            prop.prop.SetActive(false); // ensure all props are disabled at start
        }
    }

    private void EnablePropById(int id)
    {
        if (_byId.TryGetValue(id, out var prop) && prop.prop != null)
            prop.prop.SetActive(true);
    }

}
