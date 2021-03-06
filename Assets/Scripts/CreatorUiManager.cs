using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TypeReferences;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static SerializeDataClasses;

public class CreatorUiManager: MonoBehaviour
{
    public Canvas canvas;
    public TextUi textUi;
    public SliderUi sliderUi;
    public PropertiesUi behaviourPropertyUi;
    public TabButton behaviourButtonUi;
    public TabButton objectButtonUi;
    public RectTransform objectButtonParent;
    public RectTransform behaviourTab;
    public RectTransform behaviourSection;
    public Button saveButton;
    public Button testModeButton;

    private Dictionary<TabButton, PropertiesUi> propertyUiByButton;
    private List<TabButton> selectObjectsBtn;

    public void CreateUiForBehaviours(Dictionary<TypeReference, JsonData> inGameBehaviours)
    {
        propertyUiByButton = new Dictionary<TabButton, PropertiesUi>(inGameBehaviours.Count);
        
        foreach (var behaviour in inGameBehaviours)
        {
            var newBehaviourTabBtn = Instantiate(behaviourButtonUi, behaviourTab);
            var newBehaviourPropertyUi = Instantiate(behaviourPropertyUi, behaviourSection);
            
            newBehaviourTabBtn.ChangeName(behaviour.Key.Type.Name);
            newBehaviourPropertyUi.SetAttachToEntityToggleCallback(behaviour.Value);
            
            propertyUiByButton.Add(newBehaviourTabBtn, newBehaviourPropertyUi);
            
            foreach (var fieldInfo in behaviour.Key.Type.GetFields())
            {
                CreateUiForBehaviourField(fieldInfo, newBehaviourPropertyUi.transform, behaviour.Value);
            }
        }

        OnTabButtonClick(propertyUiByButton.Keys.First());
    }

    public void OnTabButtonClick(TabButton tabButtonClicked)
    {
        foreach (var tab in propertyUiByButton.Keys)
        {
            propertyUiByButton[tab].gameObject.SetActive(tab == tabButtonClicked);
        }
    }

    public void CreateUiForBehaviourField(FieldInfo fieldInfo, Transform parent, JsonData associatedJobject)
    {
        if (fieldInfo.FieldType == typeof(int))
        {
            Instantiate(sliderUi, parent).SetupSliderForIntType(fieldInfo.Name, 100, associatedJobject.jObject.Property(fieldInfo.Name));
        }
        else if (fieldInfo.FieldType == typeof(float))
        {
            Instantiate(sliderUi, parent).SetupSliderForFloatType(fieldInfo.Name, 100f, associatedJobject.jObject.Property(fieldInfo.Name));
        }
        else if (fieldInfo.FieldType == typeof(string))
        { 
            Instantiate(textUi, parent).SetupTextUi(fieldInfo.Name, "", associatedJobject.jObject.Property(fieldInfo.Name));
        }
    }

    public void SetupSaveCallback(Action saveCustomizedEntity)
    {
        saveButton.onClick.AddListener(() => { saveCustomizedEntity?.Invoke(); });
    }
    
    public void SetupTestModeCallback(Action callback)
    {
        testModeButton.onClick.AddListener(() =>
        {
            canvas.enabled = false;
            callback?.Invoke();
        });
    }
    
    public void ToggleVisible(bool toggle)
    {
        canvas.enabled = toggle;
    }

    public void CreateUiForObjects(AssetReferenceGameObject[] inGameObjects, Action<AssetReferenceGameObject> onSelectCallback)
    {
        selectObjectsBtn = new List<TabButton>(inGameObjects.Length);

        foreach (var inGameObject in inGameObjects)
        {
            var objectSelectBtn = Instantiate(objectButtonUi, objectButtonParent);
            objectSelectBtn.onClick.AddListener(() =>
            {
                onSelectCallback.Invoke(inGameObject);
                foreach (var button in selectObjectsBtn)
                {
                    button.ToggleHighlight(objectSelectBtn == button);
                }
            });
            
            selectObjectsBtn.Add(objectSelectBtn);
        }
    }
}