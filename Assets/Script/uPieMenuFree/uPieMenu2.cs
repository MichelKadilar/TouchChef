// Decompiled with JetBrains decompiler
// Type: uPIe.uPIeMenu
// Assembly: uPIeMenu, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FC0667DF-F6A4-43D3-9720-048E4DBCA02D
// Assembly location: /Users/karimcharleux/Developer/TouchChef/Assets/Plugins/uPi(e) Menu/Scripts/uPIeMenu.dll
// XML documentation location: /Users/karimcharleux/Developer/TouchChef/Assets/Plugins/uPi(e) Menu/Scripts/uPIeMenu.xml

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable disable
namespace uPIe
{
  /// <summary>Contains the main uPIe menu (runtime) logic</summary>
  [RequireComponent(typeof (RectTransform))]
  [Serializable]
  public class uPIeMenu2 : MonoBehaviour
  {
    [Tooltip("Related Canvas. Can be left empty (falls back to root object)")]
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private bool controlWithGamepad;
    [Tooltip("If the controller is not moved far enough the default selectable or simply nothing (see keepSelectedOption) will be selected")]
    [SerializeField]
    private float controllerDeadzone = 0.3f;
    [SerializeField]
    private bool useCustomInputSystem;
    private Vector2 customInput;
    private bool confirmButtonDown;
    [SerializeField]
    private string confirmInputName = "Fire1";
    [SerializeField]
    private string horizontalInputName = "Horizontal";
    [SerializeField]
    private string verticalInputName = "Vertical";
    [Tooltip("the visual indicator graphic should be attached to a child of this gameobject")]
    [SerializeField]
    private Graphic indicatorGraphic;
    [SerializeField]
    private bool applyIndicatorRotation = true;
    [Tooltip("Where should the first option start? Normally left is 0 degree, so to start at the top we need to add 90 degrees offset")]
    [SerializeField]
    private float startDegOffset;
    private const float standardOffset = 90f;
    [SerializeField]
    private GameObject menuOptionPrefab;
    [Tooltip("In clockwise order")]
    [SerializeField]
    private List<Selectable> menuOptions = new List<Selectable>();
    [SerializeField]
    private int selectedPieceId = -1;
    private int prevSelectedPieceId = -1;
    [SerializeField]
    private float circleSize = 360f;
    [SerializeField]
    private bool constrainIndicatorPosition = true;
    [Tooltip("If set to true the most recently selected option stays selected - even if the controller is back at \"origin\"")]
    [SerializeField]
    private bool keepSelectedOption = true;
    [SerializeField]
    private Selectable defaultSelected;
    [Tooltip("If the menu is not a full circle, should the \"border\" options get deselected, when not aiming directly at them?")]
    [SerializeField]
    private bool deselectOptionIfOutsideBorders;
    [SerializeField]
    private float alignRadius = 60f;
    [SerializeField]
    private bool alignRotation = true;
    [SerializeField]
    private Vector3 alignUpDirection = Vector3.up;
    [SerializeField]
    private Vector3 alignForwardDirection = Vector3.forward;
    private bool enableSelecting = true;
    private Vector2 currentDirection;
    private Selectable currentlyActiveOption;
    private bool currentlyHoveringWithMouse;
    public bool DoDrawGizmos = true;
    public bool DrawOnlyOnSelected;
    public float BoundaryLength = 1f;
    public bool DebugFoldout;
    public bool SetCircleSizeDirectly;
    public int SelectedCircleSizeId;
    public bool ButtonsFoldout = true;
    public bool UseControllerDeadzoneSlider = true;

    /// <summary>
    ///     Gets or sets the controller deadzone. This means, when is the analogue stick
    ///     of the controller considered to be centered.
    /// </summary>
    /// <value>The controller deadzone.</value>
    public float ControllerDeadzone
    {
      get => this.controllerDeadzone;
      set => this.controllerDeadzone = value;
    }

    /// <summary>
    ///     Gets or sets a value controlling whether selecting menu options is enabled or not.
    ///     This is mostly used to enable or disable a parent menu when a submenu is
    ///     opened or closed.
    /// </summary>
    /// <value>
    ///   <c>true</c> if it should be able to select the options from this menu; otherwise, <c>false</c>.
    /// </value>
    public bool EnableSelecting
    {
      get => this.enableSelecting;
      set => this.enableSelecting = value;
    }

    /// <summary>
    ///     Gets or sets the option (button or other selectable) that is selected by default.
    ///     This means this is what is selected, when the controllers analogue stick is centered
    /// </summary>
    public Selectable DefaultSelected
    {
      get => this.defaultSelected;
      set
      {
        this.defaultSelected = value;
        if (!((UnityEngine.Object) this.defaultSelected != (UnityEngine.Object) null))
          return;
        this.defaultSelected.navigation = new Navigation()
        {
          mode = Navigation.Mode.None
        };
      }
    }

    /// <summary>
    ///     If unity default input is selected (no custom input) this is what the horizontal input
    ///     is called in the input manager
    /// </summary>
    public string HorizontalInputName
    {
      get => this.horizontalInputName;
      set => this.horizontalInputName = value;
    }

    /// <summary>
    ///     If unity default input is selected (no custom input) this is what the vertical input
    ///     is called in the input manager
    /// </summary>
    public string VerticalInputName
    {
      get => this.verticalInputName;
      set => this.verticalInputName = value;
    }

    /// <summary>
    ///     If unity default input is selected (no custom input) this is what the confirm input
    ///     is called in the input manager
    /// </summary>
    public string ConfirmInputName
    {
      get => this.confirmInputName;
      set => this.confirmInputName = value;
    }

    /// <summary>
    ///     If unity default input is used this returns, if the confirm button was pressed last frame
    ///     Note: you can set this value too, but this is only recommended, if you want to create your own
    ///           customized version of uPIe
    /// </summary>
    public bool ConfirmButtonDown
    {
      get => this.confirmButtonDown;
      set => this.confirmButtonDown = value;
    }

    /// <summary>
    ///     Gets the menu option, that is currently active.
    ///     Note: you can set the value too, but this should only be done, if you
    ///           want to create your own, customized version of uPIe.
    /// </summary>
    public Selectable CurrentlyActiveOption
    {
      get => this.currentlyActiveOption;
      set => this.currentlyActiveOption = value;
    }

    /// <summary>
    ///     Gets or sets the value that determines whether to constrain the indicator position to the
    ///     nearest menu option. This only makes a difference, when using menus that are not full circle.
    /// </summary>
    public bool ConstrainIndicatorPosition
    {
      get => this.constrainIndicatorPosition;
      set => this.constrainIndicatorPosition = value;
    }

    /// <summary>Gets or sets the size of the circle menu.</summary>
    /// <value>The size of the circle.</value>
    public float CircleSize
    {
      get => this.circleSize;
      set => this.circleSize = value;
    }

    /// <summary>
    ///     Gets or sets the the value, that determines whether to keep the most recently selected option
    ///     with gamepad, when the stick is in "origin" position
    /// </summary>
    public bool KeepSelectedOption
    {
      get => this.keepSelectedOption;
      set => this.keepSelectedOption = value;
    }

    /// <summary>
    ///     Gets or sets the current direction from center of the menu
    ///     to pointer (mouse or analogue stick direction)
    /// </summary>
    /// <value>The current direction.</value>
    public Vector2 CurrentDirection
    {
      get => this.currentDirection;
      set => this.currentDirection = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether to use a custom input system or the unity default one.
    /// </summary>
    /// <value>
    ///     <c>true</c> if custom input system should be used; otherwise, <c>false</c>.
    /// </value>
    public bool UseCustomInputSystem
    {
      get => this.useCustomInputSystem;
      set => this.useCustomInputSystem = value;
    }

    /// <summary>
    ///     If we choose to use a custom input system, we need to set the direction (for analogue stick)
    ///     or position (for mouse) here
    /// </summary>
    /// <value>The custom input.</value>
    public Vector2 CustomInput
    {
      get => this.customInput;
      set => this.customInput = value;
    }

    /// <summary>Gets or sets the selected piece identifier.</summary>
    /// <value>The selected piece identifier.</value>
    public int SelectedPieceId
    {
      get => this.selectedPieceId;
      set => this.selectedPieceId = value;
    }

    /// <summary>
    ///     Gets or sets the offset in degrees where to start / where the first menu option should be
    /// </summary>
    /// <value>The start offset in degrees</value>
    public float StartDegOffset
    {
      get => this.startDegOffset;
      set => this.startDegOffset = value;
    }

    /// <summary>
    ///     Gets or sets the menu option prefab to use when creating new
    ///     menu options
    /// </summary>
    /// <value>The prefab asset to use</value>
    public GameObject MenuOptionPrefab
    {
      get => this.menuOptionPrefab;
      set => this.menuOptionPrefab = value;
    }

    /// <summary>Gets or sets the menu options.</summary>
    /// <value>The menu options.</value>
    public List<Selectable> MenuOptions
    {
      get => this.menuOptions;
      set => this.menuOptions = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether to apply indicator rotation or not.
    /// </summary>
    /// <value>
    ///     <c>true</c> if indicator rotation should be applied; otherwise, <c>false</c>.
    /// </value>
    public bool ApplyIndicatorRotation
    {
      get => this.applyIndicatorRotation;
      set => this.applyIndicatorRotation = value;
    }

    /// <summary>Gets or sets the indicator graphic.</summary>
    /// <value>The indicator graphic.</value>
    public Graphic IndicatorGraphic
    {
      get => this.indicatorGraphic;
      set => this.indicatorGraphic = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether to control with gamepad or not.
    /// </summary>
    /// <value>
    ///   <c>true</c> if controlling with gamepad; otherwise, <c>false</c>.
    /// </value>
    public bool ControlWithGamepad
    {
      get => this.controlWithGamepad;
      set => this.controlWithGamepad = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether to deselect option if outside the menu borders.
    ///     Note: only makes a difference when using menus that are not full circle
    /// </summary>
    /// <value>
    ///     <c>true</c> if the menu option should be deselected when outside the menus borders; otherwise, <c>false</c>.
    /// </value>
    public bool DeselectOptionIfOutsideBorders
    {
      get => this.deselectOptionIfOutsideBorders;
      set => this.deselectOptionIfOutsideBorders = value;
    }

    /// <summary>
    ///     Gets or sets the align radius (align helper in the inspector)
    /// </summary>
    /// <value>The align radius.</value>
    public float AlignRadius
    {
      get => this.alignRadius;
      set => this.alignRadius = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the align helper (inspector) should also align rotation or not.
    /// </summary>
    /// <value>
    ///   <c>true</c> if menu option rotation should be aligned; otherwise, <c>false</c>.
    /// </value>
    public bool AlignRotation
    {
      get => this.alignRotation;
      set => this.alignRotation = value;
    }

    /// <summary>Gets or sets the up direction used for alignment.</summary>
    /// <value>The align up direction.</value>
    public Vector3 AlignUpDirection
    {
      get => this.alignUpDirection;
      set => this.alignUpDirection = value;
    }

    /// <summary>
    ///     Gets or sets the forward direction used for alignment.
    /// </summary>
    /// <value>The align forward direction.</value>
    public Vector3 AlignForwardDirection
    {
      get => this.alignForwardDirection;
      set => this.alignForwardDirection = value;
    }

    /// <summary>
    ///     Gets the current direction from the center of the menu to the pointer (analogue stick direction or mouse position).
    /// </summary>
    /// <returns></returns>
    public Vector2 GetDirection()
    {
      Vector2 vector2 = Vector2.zero;
      vector2 = this.controlWithGamepad ? new Vector2(this.CustomInput.x, this.CustomInput.y) : this.GetMousePosition() - new Vector2(this.transform.position.x, this.transform.position.y);
      return vector2.normalized;
    }

    /// <summary>Polls the input.</summary>
    public void PollInput()
    {
      if (this.useCustomInputSystem)
        return;
      this.ConfirmButtonDown = Input.GetButtonDown(this.ConfirmInputName);
      if (!this.ControlWithGamepad)
        return;
      this.CustomInput = new Vector2(Input.GetAxisRaw(this.HorizontalInputName), Input.GetAxisRaw(this.VerticalInputName));
    }

    /// <summary>Gets the indicator position.</summary>
    /// <param name="resultPosition">The resulting position.</param>
    /// <returns></returns>
    public Vector2 GetIndicatorPosition(out Vector2 resultPosition)
    {
      return this.GetIndicatorPosition(this.GetDirection(), out resultPosition);
    }

    /// <summary>Gets the indicator position.</summary>
    /// <param name="dir">The direction.</param>
    /// <param name="resultPosition">The resulting position.</param>
    /// <returns></returns>
    public Vector2 GetIndicatorPosition(Vector2 dir, out Vector2 resultPosition)
    {
      if ((UnityEngine.Object) this.IndicatorGraphic == (UnityEngine.Object) null)
      {
        resultPosition = new Vector2();
        return Vector2.zero;
      }
      RectTransform rectTransform = (RectTransform) this.IndicatorGraphic.transform.parent;
      if ((UnityEngine.Object) rectTransform == (UnityEngine.Object) null)
        rectTransform = (RectTransform) this.transform;
      Vector2 indicatorPosition = Vector2.zero;
      if (this.ConstrainIndicatorPosition && (double) this.CircleSize < 360.0)
      {
        Vector3 startDirection = this.GetStartDirection();
        Vector3 endDirection = this.GetEndDirection();
        float signedAngle1 = this.GetSignedAngle(startDirection, (Vector3) dir);
        float signedAngle2 = this.GetSignedAngle(endDirection, (Vector3) dir);
        if ((double) signedAngle1 < 0.0)
          signedAngle1 += 360f;
        float signedAngle3 = this.GetSignedAngle(Quaternion.Euler(0.0f, 0.0f, (360f - this.CircleSize) / 2f) * startDirection, (Vector3) dir);
        if ((double) signedAngle1 > (double) this.CircleSize && (double) signedAngle3 > 0.0)
          indicatorPosition = dir = (Vector2) startDirection;
        else if ((double) signedAngle2 > 0.0 && (double) signedAngle3 < 0.0)
          indicatorPosition = dir = (Vector2) endDirection;
      }
      Vector2 b = new Vector2(rectTransform.sizeDelta.x * rectTransform.pivot.x, rectTransform.sizeDelta.y * rectTransform.pivot.y);
      resultPosition = Vector2.Scale(dir, b);
      return indicatorPosition;
    }

    /// <summary>
    ///     Confirms the current selection (simulates a click respectively button down on the currently selected
    ///     menu option).
    /// </summary>
    public void ConfirmCurrentSelection()
    {
      if ((UnityEngine.Object) this.CurrentlyActiveOption == (UnityEngine.Object) null)
        return;
      ExecuteEvents.Execute<IPointerClickHandler>(this.CurrentlyActiveOption.gameObject, (BaseEventData) new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
    }

    /// <summary>Gets the signed angle between two directions.</summary>
    /// <param name="from">From.</param>
    /// <param name="to">To.</param>
    /// <returns></returns>
    private float GetSignedAngle(Vector3 from, Vector3 to)
    {
      return Mathf.DeltaAngle(Mathf.Atan2(to.y, to.x) * 57.29578f, Mathf.Atan2(from.y, from.x) * 57.29578f);
    }

    /// <summary>
    ///     Returns the Canvas-Component related to this uPIe Menu.
    ///     Reads <see cref="F:uPIe.uPIeMenu.canvas" /> field (private), falls back to the root object if not provided
    /// </summary>
    /// <returns></returns>
    public Canvas GetCanvas()
    {
      if ((UnityEngine.Object) this.canvas != (UnityEngine.Object) null)
        return this.canvas;
      this.canvas = this.transform.root.GetComponent<Canvas>();
      return this.canvas;
    }

    /// <summary>Gets the mouse position.</summary>
    /// <returns></returns>
    private Vector2 GetMousePosition()
    {
      Canvas canvas = this.GetCanvas();
      if ((UnityEngine.Object) canvas == (UnityEngine.Object) null)
        return Vector2.zero;
      Vector2 screenPoint = this.UseCustomInputSystem ? this.CustomInput : new Vector2(Input.mousePosition.x, Input.mousePosition.y);
      if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        return screenPoint;
      Vector2 localPoint;
      RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPoint, canvas.worldCamera, out localPoint);
      return (Vector2) canvas.transform.TransformPoint((Vector3) localPoint);
    }

    /// <summary>
    ///     Gets the angle from a direction by offset and clamps between 0 and 360.
    ///     (starts at 0 again, if bigger than 360)
    /// </summary>
    /// <param name="dir">The direction.</param>
    /// <param name="degOffset">The offset in degrees.</param>
    /// <param name="zeroTo360">if set to <c>true</c> the resulting angle will not get bigger than 360�.</param>
    /// <returns></returns>
    private float GetAngle(Vector2 dir, float degOffset = 0.0f, bool zeroTo360 = false)
    {
      Vector2 offsettedDir = this.GetOffsettedDir(dir, degOffset);
      float angle = Mathf.Atan2(offsettedDir.y, offsettedDir.x) * 57.29578f;
      if (zeroTo360)
        angle = Mathf.Abs(angle - 180f);
      return angle;
    }

    /// <summary>Gets the offsetted direction.</summary>
    /// <param name="dir">The direction.</param>
    /// <param name="offset">The offset.</param>
    /// <returns></returns>
    private Vector2 GetOffsettedDir(Vector2 dir, float offset)
    {
      return (Vector2) (Quaternion.Euler(0.0f, 0.0f, offset + 90f) * (Vector3) dir);
    }

    /// <summary>Gets the piece (menu option) by angle.</summary>
    /// <param name="angle">The angle.</param>
    /// <returns></returns>
    private int GetPieceByAngle(float angle)
    {
      if (this.menuOptions.Count <= 0)
        return -1;
      angle = Mathf.Clamp(angle, 0.0f, this.CircleSize);
      float num1 = this.CircleSize / (float) this.menuOptions.Count;
      int num2 = (int) ((double) angle / (double) num1);
      return num2 >= this.menuOptions.Count ? -1 : num2;
    }

    /// <summary>Adds the menu option callback.</summary>
    /// <param name="trigger">The trigger.</param>
    private void AddMenuOptionCallback(Selectable target)
    {
      uPIeEventTrigger trigger = this.GetOrAdduPieEventTrigger(target);
      trigger.SubmitEvent += (Action<BaseEventData>) (e =>
      {
        Button component = trigger.gameObject.GetComponent<Button>();
        if ((UnityEngine.Object) component == (UnityEngine.Object) null || this.MenuOptions.IndexOf((Selectable) component) != this.SelectedPieceId)
          return;
        component.Select();
      });
      trigger.PointerEnterEvent += (Action<PointerEventData>) (e => this.currentlyHoveringWithMouse = true);
      trigger.PointerExitEvent += (Action<PointerEventData>) (e => this.currentlyHoveringWithMouse = false);
    }

    /// <summary>Adds the default selectable callback.</summary>
    /// <param name="trigger">The trigger.</param>
    private void AddDefaultSelectableCallback(Selectable target)
    {
      uPIeEventTrigger adduPieEventTrigger = this.GetOrAdduPieEventTrigger(target);
      adduPieEventTrigger.PointerEnterEvent += (Action<PointerEventData>) (e =>
      {
        if (!((UnityEngine.Object) this.DefaultSelected != (UnityEngine.Object) null))
          return;
        this.CurrentlyActiveOption = this.DefaultSelected;
        this.DefaultSelected.Select();
        this.SelectedPieceId = -1;
        this.EnableSelecting = false;
      });
      adduPieEventTrigger.PointerExitEvent += (Action<PointerEventData>) (e => this.EnableSelecting = true);
    }

    /// <summary>Creates the callbacks.</summary>
    private void CreateCallbacks()
    {
      for (int index = 0; index < this.MenuOptions.Count; ++index)
      {
        if (!((UnityEngine.Object) this.MenuOptions[index] == (UnityEngine.Object) null))
          this.AddMenuOptionCallback(this.MenuOptions[index]);
      }
      if (!((UnityEngine.Object) this.DefaultSelected != (UnityEngine.Object) null))
        return;
      this.AddDefaultSelectableCallback(this.DefaultSelected);
    }

    /// <summary>
    ///     Selects the related option. The id stored in the field <see cref="P:uPIe.uPIeMenu.SelectedPieceId" />
    ///     is used.
    /// </summary>
    /// <returns></returns>
    public Selectable SelectRelatedOption() => this.SelectRelatedOption(this.SelectedPieceId);

    /// <summary>Selects the related option by a given id.</summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    public Selectable SelectRelatedOption(int id)
    {
      try
      {
        if (id >= this.menuOptions.Count || id < 0)
          return (Selectable) null;
        Selectable menuOption = this.menuOptions[id];
        if ((UnityEngine.Object) menuOption == (UnityEngine.Object) null)
          return (Selectable) null;
        menuOption.Select();
        return menuOption;
      }
      catch (Exception ex)
      {
        return (Selectable) null;
      }
    }

    /// <summary>Removes the indicator.</summary>
    public void RemoveIndicator()
    {
      if ((UnityEngine.Object) this.IndicatorGraphic == (UnityEngine.Object) null)
        return;
      Transform parent = this.indicatorGraphic.transform.parent;
      if ((UnityEngine.Object) parent != (UnityEngine.Object) this.transform && (UnityEngine.Object) parent != (UnityEngine.Object) null)
        UnityEngine.Object.DestroyImmediate((UnityEngine.Object) parent.gameObject);
      else
        UnityEngine.Object.DestroyImmediate((UnityEngine.Object) this.IndicatorGraphic.gameObject);
    }

    /// <summary>Gets the start direction.</summary>
    /// <param name="additionalOffset">The additional offset.</param>
    /// <returns></returns>
    public Vector3 GetStartDirection(float additionalOffset = 0.0f)
    {
      return (Quaternion.Euler(0.0f, 0.0f, 90f - this.startDegOffset - additionalOffset) * new Vector3(1f, 0.0f, 0.0f)).normalized;
    }

    /// <summary>Gets the end direction.</summary>
    /// <param name="startDir">The start dir.</param>
    /// <returns></returns>
    public Vector3 GetEndDirection(Vector3 startDir)
    {
      return Quaternion.Euler(0.0f, 0.0f, -this.CircleSize) * startDir;
    }

    /// <summary>Gets the end direction.</summary>
    /// <returns></returns>
    public Vector3 GetEndDirection()
    {
      return Quaternion.Euler(0.0f, 0.0f, -this.CircleSize) * this.GetStartDirection();
    }

    /// <summary>
    ///     Adds a new button (without adding it to the menu option list).
    ///     In most cases you should use <see cref="M:uPIe.uPIeMenu.AddMenuOption" /> as
    ///     this sets up the button correctly and adds it to the menu options list
    /// </summary>
    /// <param name="name">A name for the button.</param>
    /// <param name="tryCopyFromLastMenuOption">
    ///     If set to true (default) this method will try to copy
    ///     the most recently added menu option button
    /// </param>
    /// <returns></returns>
    public Button AddButton(string name = "", bool tryCopyFromLastMenuOption = true)
    {
      GameObject instance = (GameObject) null;
      if ((UnityEngine.Object) this.MenuOptionPrefab != (UnityEngine.Object) null)
        instance = (GameObject) UnityEngine.Object.Instantiate((UnityEngine.Object) this.MenuOptionPrefab, Vector3.zero, Quaternion.identity);
      else if (tryCopyFromLastMenuOption && this.MenuOptions.Count > 0)
      {
        Selectable menuOption = this.MenuOptions[this.MenuOptions.Count - 1];
        if ((UnityEngine.Object) menuOption != (UnityEngine.Object) null)
          instance = (GameObject) UnityEngine.Object.Instantiate((UnityEngine.Object) menuOption.gameObject, Vector3.zero, Quaternion.identity);
      }
      if ((UnityEngine.Object) instance == (UnityEngine.Object) null)
      {
        instance = new GameObject();
        Image image = instance.AddComponent<Image>();
        Button button = instance.AddComponent<Button>();
        button.navigation = new Navigation()
        {
          mode = Navigation.Mode.None
        };
        button.image = image;
      }
      this.InitMenuOption(instance, name);
      return instance.GetComponent<Button>();
    }

    /// <summary>
    ///     Initialize a newly created menu option gameobject instance
    /// </summary>
    /// <param name="instance">The gameobject instance you want to be treated as a menu option</param>
    /// <param name="name">Name the menu option gameobject (optional)</param>
    public void InitMenuOption(GameObject instance, string name = "")
    {
      Vector3 localScale = instance.transform.localScale;
      instance.transform.SetParent(this.transform);
      instance.transform.SetAsLastSibling();
      instance.transform.localPosition = Vector3.zero;
      instance.transform.localRotation = Quaternion.identity;
      instance.transform.localScale = localScale;
      instance.name = string.IsNullOrEmpty(name) ? "uPIeMenuOptionButton#" + (object) this.MenuOptions.Count : name;
      Selectable target = instance.GetComponent<Selectable>();
      if ((UnityEngine.Object) target == (UnityEngine.Object) null)
        target = (Selectable) instance.AddComponent<Button>();
      this.AddMenuOptionCallback(target);
    }

    /// <summary>Adds a new menu option (button) to the menu.</summary>
    /// <returns>The newly added menu option button</returns>
    public Button AddMenuOption()
    {
      Button button = this.AddButton();
      this.MenuOptions.Add((Selectable) button);
      return button;
    }

    /// <summary>
    ///     <see cref="M:uPIe.uPIeMenu.AddMenuOptionAndRealign(System.Boolean,System.Boolean,System.Boolean)" /> takes more options than this, so from a script
    ///     you should probably use that method. But unity only allows one parameter when calling
    ///     methods from an event trigger (like OnClick) in the inspector, so if you want to
    ///     do that, use this method.
    /// </summary>
    public void AddMenuOptionAndRescaleX() => this.AddMenuOptionAndRealign();

    /// <summary>
    ///     <see cref="M:uPIe.uPIeMenu.AddMenuOptionAndRealign(System.Boolean,System.Boolean,System.Boolean)" /> takes more options than this, so from a script
    ///     you should probably use that method. But unity only allows one parameter when calling
    ///     methods from an event trigger (like OnClick) in the inspector, so if you want to
    ///     do that, use this method.
    /// </summary>
    public void AddMenuOptionAndRescaleY() => this.AddMenuOptionAndRealign(false, true);

    /// <summary>
    ///     <see cref="M:uPIe.uPIeMenu.AddMenuOptionAndRealign(System.Boolean,System.Boolean,System.Boolean)" /> takes more options than this, so from a script
    ///     you should probably use that method. But unity only allows one parameter when calling
    ///     methods from an event trigger (like OnClick) in the inspector, so if you want to
    ///     do that, use this method.
    /// </summary>
    public void AddMenuOptionAndRescaleZ()
    {
      this.AddMenuOptionAndRealign(false, autoRescaleZ: true);
    }

    /// <summary>
    ///     Adds a new menu option (button) to the menu and automatically realigns the menu options
    ///     according to what is set up in the alignment options.
    ///     See <see cref="P:uPIe.uPIeMenu.AlignRadius" />, <see cref="P:uPIe.uPIeMenu.AlignRotation" />, <see cref="P:uPIe.uPIeMenu.AlignUpDirection" />, <see cref="P:uPIe.uPIeMenu.AlignForwardDirection" />
    /// </summary>
    /// <param name="autoRescaleX">if set to <c>true</c> automatically rescales the x value.</param>
    /// <param name="autoRescaleY">if set to <c>true</c> automatically rescales the y value.</param>
    /// <param name="autoRescaleZ">if set to <c>true</c> automatically rescales the z value.</param>
    /// <returns>The newly added menu option button</returns>
    public Button AddMenuOptionAndRealign(bool autoRescaleX = true, bool autoRescaleY = false, bool autoRescaleZ = false)
    {
      Button button = this.AddMenuOption();
      if (this.MenuOptions.Count > 1)
      {
        Selectable menuOption = this.MenuOptions[this.MenuOptions.Count - 2];
        if ((UnityEngine.Object) menuOption != (UnityEngine.Object) null)
          button.transform.localScale = menuOption.transform.localScale;
      }
      float optionScaleRatio = this.GetMenuOptionScaleRatio(true);
      this.Realign();
      this.RescaleMenuOptions(autoRescaleX ? optionScaleRatio : 1f, autoRescaleY ? optionScaleRatio : 1f, autoRescaleZ ? optionScaleRatio : 1f);
      return button;
    }

    /// <summary>
    ///     <see cref="M:uPIe.uPIeMenu.RemoveMenuOptionAndRealign(System.Boolean,System.Boolean,System.Boolean)" /> takes more options than this, so from a script
    ///     you should probably use that method. But unity only allows one parameter when calling
    ///     methods from an event trigger (like OnClick) in the inspector, so if you want to
    ///     do that, use this method.
    /// </summary>
    public void RemoveMenuOptionAndRescaleX() => this.RemoveMenuOptionAndRealign();

    /// <summary>
    ///     <see cref="M:uPIe.uPIeMenu.RemoveMenuOptionAndRealign(System.Boolean,System.Boolean,System.Boolean)" /> takes more options than this, so from a script
    ///     you should probably use that method. But unity only allows one parameter when calling
    ///     methods from an event trigger (like OnClick) in the inspector, so if you want to
    ///     do that, use this method.
    /// </summary>
    public void RemoveMenuOptionAndRescaleY() => this.RemoveMenuOptionAndRealign(false, true);

    /// <summary>
    ///     <see cref="M:uPIe.uPIeMenu.RemoveMenuOptionAndRealign(System.Boolean,System.Boolean,System.Boolean)" /> takes more options than this, so from a script
    ///     you should probably use that method. But unity only allows one parameter when calling
    ///     methods from an event trigger (like OnClick) in the inspector, so if you want to
    ///     do that, use this method.
    /// </summary>
    public void RemoveMenuOptionAndRescaleZ()
    {
      this.RemoveMenuOptionAndRealign(false, autoRescaleZ: true);
    }

    /// <summary>
    ///     Removes the last menu option (button) from the menu and automatically realigns the menu options
    ///     according to what is set up in the alignment options.
    ///     See <see cref="P:uPIe.uPIeMenu.AlignRadius" />, <see cref="P:uPIe.uPIeMenu.AlignRotation" />, <see cref="P:uPIe.uPIeMenu.AlignUpDirection" />, <see cref="P:uPIe.uPIeMenu.AlignForwardDirection" />
    /// </summary>
    /// <param name="autoRescaleX">if set to <c>true</c> automatically rescales the x value.</param>
    /// <param name="autoRescaleY">if set to <c>true</c> automatically rescales the y value.</param>
    /// <param name="autoRescaleZ">if set to <c>true</c> automatically rescales the z value.</param>
    /// <returns>The newly added menu option button</returns>
    public void RemoveMenuOptionAndRealign(bool autoRescaleX = true, bool autoRescaleY = false, bool autoRescaleZ = false)
    {
      this.RemoveMenuOption();
      float optionScaleRatio = this.GetMenuOptionScaleRatio(false);
      this.Realign();
      this.RescaleMenuOptions(autoRescaleX ? optionScaleRatio : 1f, autoRescaleY ? optionScaleRatio : 1f, autoRescaleZ ? optionScaleRatio : 1f);
    }

    /// <summary>Removes the most recently added menu option.</summary>
    public void RemoveMenuOption() => this.RemoveMenuOption(this.MenuOptions.Count - 1);

    /// <summary>Removes the menu option by a given id.</summary>
    /// <param name="id">The identifier.</param>
    public void RemoveMenuOption(int id)
    {
      if (id < 0 || id >= this.MenuOptions.Count)
        return;
      this.RemoveMenuOption(this.MenuOptions[id]);
    }

    /// <summary>Removes the menu option by a given selectable.</summary>
    /// <param name="slct">The selectable to remove.</param>
    public void RemoveMenuOption(Selectable slct)
    {
      if ((UnityEngine.Object) slct == (UnityEngine.Object) null)
        return;
      UnityEngine.Object.DestroyImmediate((UnityEngine.Object) slct.gameObject);
      this.MenuOptions.Remove(slct);
    }

    /// <summary>Clears all menu options.</summary>
    public void ClearMenuOptions()
    {
      foreach (Selectable menuOption in this.MenuOptions)
      {
        if (!((UnityEngine.Object) menuOption == (UnityEngine.Object) null))
          UnityEngine.Object.DestroyImmediate((UnityEngine.Object) menuOption.gameObject);
      }
      if ((UnityEngine.Object) this.DefaultSelected != (UnityEngine.Object) null)
        UnityEngine.Object.DestroyImmediate((UnityEngine.Object) this.DefaultSelected.gameObject);
      this.MenuOptions.Clear();
    }

    /// <summary>Deselects the currently selected menu option.</summary>
    public void Deselect()
    {
      PointerEventData eventData = new PointerEventData(EventSystem.current);
      for (int index = 0; index < this.MenuOptions.Count; ++index)
      {
        if (!((UnityEngine.Object) this.MenuOptions[index] == (UnityEngine.Object) null))
          ExecuteEvents.Execute<IDeselectHandler>(this.MenuOptions[index].gameObject, (BaseEventData) eventData, ExecuteEvents.deselectHandler);
      }
      this.CurrentlyActiveOption = (Selectable) null;
      EventSystem.current.SetSelectedGameObject((GameObject) null);
    }

    /// <summary>Opens a sub-uPIe-menu.</summary>
    /// <param name="subMenu">The sub menu.</param>
    public void OpenSubMenu(uPIeMenu subMenu)
    {
      this.EnableSelecting = false;
      subMenu.gameObject.SetActive(true);
      subMenu.enabled = true;
      subMenu.EnableSelecting = true;
      for (int index = 0; index < this.MenuOptions.Count; ++index)
      {
        if (!((UnityEngine.Object) this.MenuOptions[index] == (UnityEngine.Object) null))
          this.MenuOptions[index].interactable = false;
      }
    }

    /// <summary>
    ///     Closes this sub-uPIe-menu and retuns to the uPIe-menu that
    ///     is superordinated to this one.
    /// </summary>
    /// <param name="superMenu">The super menu.</param>
    public void ReturnToSuperMenu(uPIeMenu superMenu)
    {
      this.gameObject.SetActive(false);
      superMenu.EnableSelecting = true;
      superMenu.enabled = true;
      superMenu.gameObject.SetActive(true);
      for (int index = 0; index < superMenu.MenuOptions.Count; ++index)
      {
        if (!((UnityEngine.Object) superMenu.MenuOptions[index] == (UnityEngine.Object) null))
          superMenu.MenuOptions[index].interactable = true;
      }
    }

    /// <summary>
    ///     Realigns all menu options.
    ///     The options set up as alignment options are used.
    ///     See <see cref="P:uPIe.uPIeMenu.AlignRadius" />, <see cref="P:uPIe.uPIeMenu.AlignRotation" />, <see cref="P:uPIe.uPIeMenu.AlignUpDirection" />, <see cref="P:uPIe.uPIeMenu.AlignForwardDirection" />
    /// </summary>
    public void Realign()
    {
      this.Realign(this.AlignRadius, this.AlignRotation, this.AlignUpDirection, this.AlignForwardDirection);
    }

    /// <summary>
    ///     Realigns all menu options by only using a different radius as set up in the alignment options.
    ///     See <see cref="P:uPIe.uPIeMenu.AlignRadius" />, <see cref="P:uPIe.uPIeMenu.AlignRotation" />, <see cref="P:uPIe.uPIeMenu.AlignUpDirection" />, <see cref="P:uPIe.uPIeMenu.AlignForwardDirection" />
    /// </summary>
    /// <param name="radius">The radius to align all options along.</param>
    public void Realign(float radius)
    {
      this.Realign(radius, this.AlignRotation, this.AlignUpDirection, this.alignForwardDirection);
    }

    /// <summary>Realigns all menu options by given options</summary>
    /// <param name="radius">The radius.</param>
    /// <param name="doAlignRotation">if set to <c>true</c> the menu options are also rotated.</param>
    /// <param name="upDirection">Local up direction of the menu options.</param>
    /// <param name="forwardDirection">Local forward direction of the menu options.</param>
    public void Realign(
      float radius,
      bool doAlignRotation,
      Vector3 upDirection,
      Vector3 forwardDirection)
    {
      RectTransform transform1 = (RectTransform) this.transform;
      float num = this.CircleSize / (float) this.MenuOptions.Count;
      Vector3 startDirection = this.GetStartDirection(num / 2f);
      for (int index = 0; index < this.MenuOptions.Count; ++index)
      {
        Vector3 direction = Quaternion.Euler(0.0f, 0.0f, -num * (float) index) * startDirection;
        Vector3 vector3 = transform1.TransformDirection(direction);
        Transform transform2 = this.MenuOptions[index].transform;
        transform2.position = transform1.position + vector3 * radius;
        if (doAlignRotation)
        {
          transform2.rotation = Quaternion.LookRotation(transform1.TransformDirection(forwardDirection), vector3.normalized);
          transform2.rotation = Quaternion.LookRotation(transform2.forward, transform2.TransformDirection(upDirection));
        }
      }
    }

    /// <summary>Rescales all menu options.</summary>
    /// <param name="xScale">The x scale.</param>
    /// <param name="yScale">The y scale.</param>
    /// <param name="zScale">The z scale.</param>
    /// <param name="multiply">
    ///     if set to <c>true</c> the scale is multiplied by its current scale. If set to <c>false</c>
    ///     the scale value is directly set.
    /// </param>
    public void RescaleMenuOptions(float xScale, float yScale = 1f, float zScale = 1f, bool multiply = true)
    {
      for (int index = 0; index < this.MenuOptions.Count; ++index)
      {
        RectTransform transform = (RectTransform) this.MenuOptions[index].transform;
        Vector3 localScale = transform.localScale;
        if (multiply)
        {
          localScale.x *= xScale;
          localScale.y *= yScale;
          localScale.z *= zScale;
        }
        else
        {
          localScale.x = xScale;
          localScale.y = yScale;
          localScale.z = zScale;
        }
        transform.localScale = localScale;
      }
    }

    private void Awake() => this.CreateCallbacks();

    private void Update()
    {
      if (!this.EnableSelecting && !this.ControlWithGamepad)
        return;
      this.PollInput();
      this.currentDirection = this.GetDirection();
      if (this.ControlWithGamepad)
      {
        if ((double) this.CustomInput.sqrMagnitude < (double) this.ControllerDeadzone)
        {
          if (!((UnityEngine.Object) this.DefaultSelected != (UnityEngine.Object) null) && this.keepSelectedOption)
            return;
          if ((UnityEngine.Object) this.DefaultSelected != (UnityEngine.Object) null && (UnityEngine.Object) this.CurrentlyActiveOption != (UnityEngine.Object) this.DefaultSelected)
          {
            this.CurrentlyActiveOption = this.DefaultSelected;
            this.DefaultSelected.Select();
          }
          if (!this.keepSelectedOption)
          {
            this.Deselect();
            this.CurrentlyActiveOption = (Selectable) null;
          }
          this.prevSelectedPieceId = -1;
          this.SelectedPieceId = -1;
          if (!((UnityEngine.Object) this.IndicatorGraphic != (UnityEngine.Object) null))
            return;
          this.IndicatorGraphic.enabled = false;
          return;
        }
        if ((UnityEngine.Object) this.IndicatorGraphic != (UnityEngine.Object) null && !this.IndicatorGraphic.enabled)
          this.IndicatorGraphic.enabled = true;
      }
      this.SelectedPieceId = this.GetPieceByAngle(this.GetAngle(this.currentDirection, this.StartDegOffset, true));
      if (this.SelectedPieceId != this.prevSelectedPieceId)
      {
        if (this.SelectedPieceId >= 0)
          this.CurrentlyActiveOption = this.SelectRelatedOption();
        else if (this.DeselectOptionIfOutsideBorders && (double) this.CircleSize < 360.0)
          this.Deselect();
      }
      this.prevSelectedPieceId = this.SelectedPieceId;
      if (this.ConfirmButtonDown)
      {
        if ((UnityEngine.Object) this.CurrentlyActiveOption != (UnityEngine.Object) null && !this.currentlyHoveringWithMouse)
        {
          EventSystem.current.SetSelectedGameObject((GameObject) null);
          ExecuteEvents.Execute<ISubmitHandler>(this.CurrentlyActiveOption.gameObject, (BaseEventData) new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }
        this.ConfirmButtonDown = false;
      }
      if ((UnityEngine.Object) this.IndicatorGraphic == (UnityEngine.Object) null)
        return;
      Vector2 resultPosition;
      Vector2 indicatorPosition = this.GetIndicatorPosition(this.currentDirection, out resultPosition);
      if (this.ApplyIndicatorRotation)
      {
        if (indicatorPosition != Vector2.zero)
          this.indicatorGraphic.transform.right = (Vector3) indicatorPosition;
        else
          this.IndicatorGraphic.transform.right = (Vector3) this.currentDirection;
      }
      this.IndicatorGraphic.transform.localPosition = (Vector3) resultPosition;
    }

    private float GetMenuOptionScaleRatio(bool scaleBigger)
    {
      if (this.MenuOptions.Count <= 1)
        return 1f;
      float num = (float) this.MenuOptions.Count / ((float) this.MenuOptions.Count - 1f);
      return !scaleBigger ? num : 1f / num;
    }

    private uPIeEventTrigger GetOrAdduPieEventTrigger(Selectable target)
    {
      uPIeEventTrigger adduPieEventTrigger = target.GetComponent<uPIeEventTrigger>();
      if ((UnityEngine.Object) adduPieEventTrigger == (UnityEngine.Object) null)
        adduPieEventTrigger = target.gameObject.AddComponent<uPIeEventTrigger>();
      return adduPieEventTrigger;
    }

    private void OnDrawGizmos()
    {
      if (!this.DoDrawGizmos || this.DrawOnlyOnSelected)
        return;
      this.DrawGizmos();
    }

    private void OnDrawGizmosSelected()
    {
      if (!this.DoDrawGizmos || !this.DrawOnlyOnSelected)
        return;
      this.DrawGizmos();
    }

    private void DrawGizmos()
    {
      Vector3 startDirection = this.GetStartDirection();
      RectTransform transform = (RectTransform) this.transform;
      float num1 = Mathf.Max(transform.sizeDelta.x, transform.sizeDelta.y);
      Vector3 b = new Vector3(num1, num1, 0.0f);
      float num2 = this.CircleSize / (float) this.menuOptions.Count;
      Gizmos.color = Color.black;
      for (int index = 0; index < this.menuOptions.Count; ++index)
        Gizmos.DrawRay(this.transform.position, this.transform.TransformDirection((Vector3) Vector2.Scale((Vector2) (Quaternion.Euler(0.0f, 0.0f, -num2 * (float) index) * startDirection), (Vector2) b)) * this.BoundaryLength);
      Gizmos.color = Color.yellow;
      Gizmos.DrawRay(this.transform.position, this.transform.TransformDirection((Vector3) Vector2.Scale((Vector2) this.GetEndDirection(startDirection), (Vector2) b)) * this.BoundaryLength);
      Gizmos.color = Color.green;
      Gizmos.DrawRay(this.transform.position, this.transform.TransformDirection((Vector3) Vector2.Scale((Vector2) startDirection, (Vector2) b)) * this.BoundaryLength);
    }

    private void OnGUI()
    {
      GUIStyle other = new GUIStyle();
      int num = Math.Min(Screen.height, Screen.width);
      other.fontSize = Math.Max((int) Math.Round((double) num / 20.0), 18);
      GUIStyle style1 = new GUIStyle(other);
      style1.fontStyle = FontStyle.Bold;
      style1.normal.textColor = new Color(0.0f, 0.0f, 0.0f, 0.6f);
      GUIStyle style2 = new GUIStyle(other);
      style2.fontStyle = FontStyle.Normal;
      style2.fontSize -= 4;
      style2.normal.textColor = new Color((float) byte.MaxValue / 256f, 45f / 128f, 1f / 128f, 0.6f);
      int x1 = 6;Canvas canvas = this.GetCanvas();
      Vector2 menuScreenPos = new Vector2();
      if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        menuScreenPos = RectTransformUtility.WorldToScreenPoint((Camera) null, this.transform.position);
      else
        menuScreenPos = (Vector2) (!(bool) (UnityEngine.Object) canvas || !(bool) (UnityEngine.Object) canvas.worldCamera ? Camera.main : canvas.worldCamera).WorldToScreenPoint(this.transform.position);
      menuScreenPos.y = (float) Screen.height - menuScreenPos.y;
    }
  }
}
