using CosmicShore.Game;
using CosmicShore.Game.AI;
using CosmicShore.Game.Animation;
using CosmicShore.Game.IO;
using CosmicShore.Game.Projectiles;
using CosmicShore.Models;
using CosmicShore.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Analytics;
using UnityEngine;

namespace CosmicShore.Core
{
    [Serializable]
    public struct InputEventShipActionMapping
    {
        public InputEvents InputEvent;
        public List<ShipAction> ShipActions;
    }

    [Serializable]
    public struct ResourceEventShipActionMapping
    {
        public ResourceEvents ResourceEvent;
        public List<ShipAction> ClassActions;
    }

    [RequireComponent(typeof(ResourceSystem))]
    [RequireComponent(typeof(TrailSpawner))]
    [RequireComponent(typeof(ShipStatus))]
    [RequireComponent(typeof(Silhouette))]
    [RequireComponent(typeof(ShipCameraCustomizer))]
    public class Ship : MonoBehaviour, IShip
    {
        public event Action OnShipInitialized;

        [SerializeField] List<ImpactProperties> impactProperties;

        [Header("Ship Meta")]
        [SerializeField] string Name;
        public string ShipName => Name;

        [SerializeField] ShipTypes ShipType;
        public ShipTypes GetShipType => ShipType;

        [Header("Ship Components")]
        [SerializeField] Skimmer nearFieldSkimmer;
        [SerializeField] GameObject OrientationHandle;

        [SerializeField] List<GameObject> shipGeometries;
        public List<GameObject> ShipGeometries => shipGeometries;

        [SerializeField] GameObject shipHUD;

        [Header("Optional Ship Components")]
        [SerializeField] GameObject AOEPrefab;
        [SerializeField] Skimmer farFieldSkimmer;

        [SerializeField] int resourceIndex = 0;
        [SerializeField] int ammoResourceIndex = 0; // TODO: move to an ability system with separate classes
        [SerializeField] int boostResourceIndex = 0; // TODO: move to an ability system with separate classes

        [Header("Environment Interactions")]
        [SerializeField] public List<CrystalImpactEffects> crystalImpactEffects;
        [ShowIf(CrystalImpactEffects.AreaOfEffectExplosion)][SerializeField] float minExplosionScale = 50; // TODO: depricate "ShowIf" once we adopt modularity
        [ShowIf(CrystalImpactEffects.AreaOfEffectExplosion)][SerializeField] float maxExplosionScale = 400;

        [SerializeField] List<TrailBlockImpactEffects> trailBlockImpactEffects;
        [SerializeField] float blockChargeChange;

        [Header("Configuration")]
        [SerializeField] float boostMultiplier = 4f; // TODO: Move to ShipController
        public float BoostMultiplier { get => boostMultiplier; set => boostMultiplier = value;}

        [SerializeField] bool bottomEdgeButtons = false;
        
        [SerializeField] float Inertia = 70f;
        public float GetInertia => Inertia;

        [SerializeField] List<InputEventShipActionMapping> inputEventShipActions;
        Dictionary<InputEvents, List<ShipAction>> _shipControlActions = new();

        [SerializeField] List<ResourceEventShipActionMapping> resourceEventClassActions;
        Dictionary<ResourceEvents, List<ShipAction>> _classResourceActions = new();

        [Serializable]
        public struct ElementStat
        {
            public string StatName;
            public Element Element;

            public ElementStat(string statName, Element element)
            {
                StatName = statName;
                Element = element;
            }
        }

        [SerializeField] List<ElementStat> ElementStats = new();

        public Transform FollowTarget { get; private set; }

        Teams _team;
        public Teams Team
        {
            get => _team;
            private set
            {
                _team = value;
                if (nearFieldSkimmer != null) nearFieldSkimmer.Team = value;
                if (farFieldSkimmer != null) farFieldSkimmer.Team = value;
            }
        }

        private AIPilot _aiPilot;
        public AIPilot AIPilot
        {
            get
            {
                _aiPilot = _aiPilot != null ? _aiPilot : gameObject.GetComponent<AIPilot>();
                return _aiPilot;
            }
        }

        ShipTransformer _shipTransformer;
        public ShipTransformer ShipTransformer
        {
            get
            {
                _shipTransformer = _shipTransformer !=  null ? _shipTransformer : GetComponent<ShipTransformer>();
                return _shipTransformer;
            }
        }

        ShipAnimation _shipAnimation;
        public ShipAnimation ShipAnimation 
        {
            get
            {
                _shipAnimation = _shipAnimation != null ? _shipAnimation : GetComponent<ShipAnimation>();
                return _shipAnimation;
            }
        }

        TrailSpawner _trailSpawner;
        public TrailSpawner TrailSpawner 
        { 
            get
            {
                _trailSpawner = _trailSpawner != null ? _trailSpawner : GetComponent<TrailSpawner>();
                return _trailSpawner;
            }
        }

        ResourceSystem _resourceSystem;
        public ResourceSystem ResourceSystem
        {
            get
            {
                _resourceSystem = _resourceSystem != null ? _resourceSystem : GetComponent<ResourceSystem>();
                return _resourceSystem;
            }
        }

        ShipStatus _shipStatus;
        public ShipStatus ShipStatus
        {
            get
            {
                _shipStatus = _shipStatus != null ? _shipStatus : GetComponent<ShipStatus>();
                return _shipStatus;
            }
        }

        Silhouette _silhouette;
        public Silhouette Silhouette
        {
            get
            {
                _silhouette = _silhouette != null ? _silhouette : GetComponent<Silhouette>();
                return _silhouette;
            }
        }

        IPlayer _player;
        public IPlayer Player 
        { 
            get => _player;
            private set
            {
                _player = value;
                if (nearFieldSkimmer != null) nearFieldSkimmer.Player = value;
                if (farFieldSkimmer != null) farFieldSkimmer.Player = value;
            }
        }

        InputController _inputController;
        public InputController InputController 
        { 
            get
            {
                if (_inputController == null)
                {
                    if (Player == null)
                    {
                        Debug.LogError($"No player found to get input controller!");
                        return null;
                    }
                    _inputController = Player.InputController;
                }
                return _inputController;
            }
        }

        ShipCameraCustomizer _shipCameraCustomizer;
        public ShipCameraCustomizer ShipCameraCustomizer
        {
            get
            {
                _shipCameraCustomizer = _shipCameraCustomizer != null ? _shipCameraCustomizer : GetComponent<ShipCameraCustomizer>();
                return _shipCameraCustomizer;
            }
        }

        public CameraManager CameraManager => CameraManager.Instance;
        public Transform Transform => transform;
        public Material AOEExplosionMaterial { get; private set; }
        public Material AOEConicExplosionMaterial { get; private set; }
        public Material SkimmerMaterial { get; private set; }
        public SO_Captain Captain { get; private set; }


        Dictionary<InputEvents, float> _inputAbilityStartTimes = new();
        Dictionary<ResourceEvents, float> _resourceAbilityStartTimes = new();
        Material _shipMaterial;
        float _speedModifierDuration = 2f;


        public void Initialize(IPlayer player, Teams team = Teams.None)
        {
            _player = player;
            _team = team;

            if (!FollowTarget) FollowTarget = transform;
            if (bottomEdgeButtons) Player.GameCanvas.MiniGameHUD.PositionButtonPanel(true);

            InitializeShipGeometries();
            InitializeShipControlActions();
            InitializeClassResourceActions();

            Silhouette.Initialize(this);
            ShipAnimation.Initialize(this);
            ShipTransformer.Initialize(this);
            AIPilot.Initialize(this);
            nearFieldSkimmer.Initialize(this);
            farFieldSkimmer.Initialize(this);
            ShipCameraCustomizer.Initialize(this);
            TrailSpawner.Initialize(this);

            if (AIPilot.AutoPilotEnabled) return;
            if (!shipHUD) return;
            shipHUD.SetActive(true);
            foreach (var child in shipHUD.GetComponentsInChildren<Transform>(false))
            {
                child.SetParent(Player.GameCanvas.transform, false);
                child.SetSiblingIndex(0);   // Don't draw on top of modal screens
            }

            OnShipInitialized?.Invoke();
        }

        void InitializeShipGeometries() => ShipHelper.InitializeShipGeometries(this, shipGeometries);
        void InitializeShipControlActions() => ShipHelper.InitializeShipControlActions(this, inputEventShipActions, _shipControlActions);
        void InitializeClassResourceActions() => ShipHelper.InitializeClassResourceActions(this, resourceEventClassActions, _classResourceActions);

        public void BindElementalFloat(string statName, Element element)
        {
            Debug.Log($"Ship.NotifyShipStatBinding - statName:{statName}, element:{element}");
            if (ElementStats.All(x => x.StatName != statName))
                ElementStats.Add(new ElementStat(statName, element));
            
            Debug.Log($"Ship.NotifyShipStatBinding - ElementStats.Count:{ElementStats.Count}");
        }

        public void PerformCrystalImpactEffects(CrystalProperties crystalProperties) // TODO: move to an ability system with separate classes
        {
            if (StatsManager.Instance != null)
                StatsManager.Instance.CrystalCollected(this, crystalProperties);

            foreach (CrystalImpactEffects effect in crystalImpactEffects)
            {
                switch (effect)
                {
                    case CrystalImpactEffects.PlayHaptics:
                        if (!ShipStatus.AutoPilotEnabled) HapticController.PlayHaptic(HapticType.CrystalCollision);//.PlayCrystalImpactHaptics();
                        break;
                    case CrystalImpactEffects.AreaOfEffectExplosion:
                        var aoeExplosion = Instantiate(AOEPrefab).GetComponent<AOEExplosion>();
                        aoeExplosion.Ship = this;
                        aoeExplosion.SetPositionAndRotation(transform.position, transform.rotation);
                        Debug.Log($"Ship.PerformCrystalImpactEffects - AOEExplosion.current ammo:{ResourceSystem.Resources[ammoResourceIndex].CurrentAmount}");
                        aoeExplosion.MaxScale = ResourceSystem.Resources.Count > ammoResourceIndex 
                            ? Mathf.Lerp(minExplosionScale, maxExplosionScale, ResourceSystem.Resources[ammoResourceIndex].CurrentAmount) : maxExplosionScale;
                        break;
                    case CrystalImpactEffects.IncrementLevel:
                        ResourceSystem.IncrementLevel(crystalProperties.Element); // TODO: consider removing here and leaving this up to the crystals
                        break;
                    case CrystalImpactEffects.FillCharge:
                        ResourceSystem.ChangeResourceAmount(boostResourceIndex, crystalProperties.fuelAmount);
                        break;
                    case CrystalImpactEffects.Boost:
                        ShipTransformer.ModifyThrottle(crystalProperties.speedBuffAmount, 4 * _speedModifierDuration);
                        break;
                    case CrystalImpactEffects.DrainAmmo:
                        ResourceSystem.ChangeResourceAmount(ammoResourceIndex, - ResourceSystem.Resources[ammoResourceIndex].CurrentAmount);
                        break;
                    case CrystalImpactEffects.GainOneThirdMaxAmmo:
                        ResourceSystem.ChangeResourceAmount(ammoResourceIndex, ResourceSystem.Resources[ammoResourceIndex].CurrentAmount / 3f);
                        break;
                    case CrystalImpactEffects.GainFullAmmo:
                        ResourceSystem.ChangeResourceAmount(ammoResourceIndex, ResourceSystem.Resources[ammoResourceIndex].MaxAmount);
                        break;
                }
            }
        }

        public void PerformTrailBlockImpactEffects(TrailBlockProperties trailBlockProperties)
        {
            foreach (TrailBlockImpactEffects effect in trailBlockImpactEffects)
            {
                switch (effect)
                {
                    case TrailBlockImpactEffects.PlayHaptics:
                        if (!ShipStatus.AutoPilotEnabled) HapticController.PlayHaptic(HapticType.BlockCollision);//.PlayBlockCollisionHaptics();
                        break;
                    case TrailBlockImpactEffects.DrainHalfAmmo:
                        ResourceSystem.ChangeResourceAmount(ammoResourceIndex, -ResourceSystem.Resources[ammoResourceIndex].CurrentAmount / 2f);
                        break;
                    case TrailBlockImpactEffects.DebuffSpeed:
                        ShipTransformer.ModifyThrottle(trailBlockProperties.speedDebuffAmount, _speedModifierDuration);
                        break;
                    case TrailBlockImpactEffects.DeactivateTrailBlock:
                        break;
                    case TrailBlockImpactEffects.ActivateTrailBlock:
                        break;
                    case TrailBlockImpactEffects.OnlyBuffSpeed:
                        if (trailBlockProperties.speedDebuffAmount > 1) ShipTransformer.ModifyThrottle(trailBlockProperties.speedDebuffAmount, _speedModifierDuration);
                        break;
                    case TrailBlockImpactEffects.GainResourceByVolume:
                        ResourceSystem.ChangeResourceAmount(boostResourceIndex, blockChargeChange);
                        break;
                    case TrailBlockImpactEffects.Attach:
                        Attach(trailBlockProperties.trailBlock);
                        ShipStatus.GunsActive = true;
                        break;
                    case TrailBlockImpactEffects.GainResource:
                        ResourceSystem.ChangeResourceAmount(resourceIndex, blockChargeChange);
                        break;
                    case TrailBlockImpactEffects.Bounce:
                        var cross = Vector3.Cross(transform.forward, trailBlockProperties.trailBlock.transform.forward);
                        var normal = Quaternion.AngleAxis(90, cross) * trailBlockProperties.trailBlock.transform.forward;
                        var reflectForward = Vector3.Reflect(transform.forward, normal);
                        var reflectUp = Vector3.Reflect(transform.up, normal);
                        ShipTransformer.GentleSpinShip(reflectForward, reflectUp, 1);
                        ShipTransformer.ModifyVelocity((transform.position - trailBlockProperties.trailBlock.transform.position).normalized * 5 , Time.deltaTime * 15);
                        break;
                    case TrailBlockImpactEffects.Explode:
                        trailBlockProperties.trailBlock.Damage(ShipStatus.Course * ShipStatus.Speed * Inertia, Team, Player.PlayerName);
                        break;
                    case TrailBlockImpactEffects.FeelDanger:
                        if (trailBlockProperties.IsDangerous && trailBlockProperties.trailBlock.Team != _team)
                        {
                            HapticController.PlayHaptic(HapticType.FakeCrystalCollision);
                            ShipTransformer.ModifyThrottle(trailBlockProperties.speedDebuffAmount, trailBlockProperties.volume / 10);                           
                        }
                        break;
                    case TrailBlockImpactEffects.Steal:
                        break;
                    case TrailBlockImpactEffects.DecrementLevel:
                        break;
                    case TrailBlockImpactEffects.Shield:
                        break;
                    case TrailBlockImpactEffects.Stop:
                        break;
                    case TrailBlockImpactEffects.Fire:
                        break;
                    case TrailBlockImpactEffects.FX:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void PerformShipControllerActions(InputEvents @event)
        {
            ShipHelper.PerformShipControllerActions(@event, out float time, _shipControlActions);
            _inputAbilityStartTimes[@event] = time;
        }

        public void StopShipControllerActions(InputEvents controlType)
        {
            if (StatsManager.Instance != null)
                StatsManager.Instance.AbilityActivated(Team, _player.PlayerName, controlType, Time.time-_inputAbilityStartTimes[controlType]);

            ShipHelper.StopShipControllerActions(controlType, _shipControlActions);
        }


        // this is used with buttons so "Find all references" will not return editor usage
        public void PerformButtonActions(int buttonNumber)
        {
            Debug.Log($"Ship.PerformButtonActions - buttonNumber:{buttonNumber}");
            
            switch (buttonNumber)
            {
                case 1:
                    if(_shipControlActions.ContainsKey(InputEvents.Button1Action))
                        PerformShipControllerActions(InputEvents.Button1Action);
                    break;
                case 2:
                    if(_shipControlActions.ContainsKey(InputEvents.Button2Action))
                        PerformShipControllerActions(InputEvents.Button2Action);
                    break;
                case 3:
                    if(_shipControlActions.ContainsKey(InputEvents.Button3Action))
                        PerformShipControllerActions(InputEvents.Button3Action);
                    break;
                default:
                    Debug.LogWarning($"Ship.PerformButtonActions - buttonNumber:{buttonNumber} is not associated to any of the ship actions.");
                    break;
            }
        }

        // this is used with buttons so "Find all references" will not return editor usage
        public void StopButtonActions(int buttonNumber)
        {
            Debug.Log($"Ship.StopButtonActions - buttonNumber:{buttonNumber}");

            switch (buttonNumber)
            {
                case 1:
                    if(_shipControlActions.ContainsKey(InputEvents.Button1Action))
                        StopShipControllerActions(InputEvents.Button1Action);
                    break;
                case 2:
                    if(_shipControlActions.ContainsKey(InputEvents.Button2Action))
                        StopShipControllerActions(InputEvents.Button2Action);
                    break;
                case 3:
                    if(_shipControlActions.ContainsKey(InputEvents.Button3Action))
                        StopShipControllerActions(InputEvents.Button3Action);
                    break;
                default:
                    Debug.LogWarning($"Ship.StopButtonActions - buttonNumber:{buttonNumber} is not associated to any of the ship actions.");
                    break;
            }
        }

        public void PerformClassResourceActions(ResourceEvents resourceEvent)
        {
            _resourceAbilityStartTimes[resourceEvent] = Time.time;

            if (!_classResourceActions.TryGetValue(resourceEvent, out var classResourceActions)) return;

            foreach (var action in classResourceActions)
                action.StartAction();
        }
     
        public void StopClassResourceActions(ResourceEvents resourceEvent)
        {
            //if (StatsManager.Instance != null)
            //    StatsManager.Instance.AbilityActivated(Team, player.PlayerName, resourceEvent, Time.time-inputAbilityStartTimes[controlType]);

            if (!_classResourceActions.TryGetValue(resourceEvent, out var classResourceActions)) return;
            foreach (var action in classResourceActions)
                action.StopAction();
        }

        public void ToggleCollision(bool enabled)
        {
            foreach (var collider in GetComponentsInChildren<Collider>(true))
                collider.enabled = enabled;
        }

        public void SetResourceLevels(ResourceCollection resourceGroup)
        {
            ResourceSystem.InitializeElementLevels(resourceGroup);
        }

        public void AssignCaptain(Captain captain)
        {
            Captain = captain.SO_Captain;
        }

        public void AssignCaptain(SO_Captain captain)
        {
            Captain = captain;
        }

        public void SetShipMaterial(Material material)
        {
            _shipMaterial = material;
            ShipHelper.ApplyShipMaterial(_shipMaterial, shipGeometries);
        }

        public void SetBlockMaterial(Material material)
        {
            TrailSpawner.SetBlockMaterial(material);
        }

        public void SetBlockSilhouettePrefab(GameObject prefab)
        {
            if (Silhouette) Silhouette.SetBlockPrefab(prefab);
        }

        public void SetShieldedBlockMaterial(Material material)
        {
            TrailSpawner.SetShieldedBlockMaterial(material);
        }

        public void SetAOEExplosionMaterial(Material material)
        {
            AOEExplosionMaterial = material;
        }

        public void SetAOEConicExplosionMaterial(Material material)
        {
            AOEConicExplosionMaterial = material;
        }

        public void SetSkimmerMaterial(Material material)
        {
            SkimmerMaterial = material;
        }

        public void FlipShipUpsideDown() // TODO: move to shipController
        {
            OrientationHandle.transform.localRotation = Quaternion.Euler(0, 0, 180);
        }

        public void FlipShipRightsideUp()
        {
            OrientationHandle.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        public void SetShipUp(float angle)
        {
            OrientationHandle.transform.localRotation = Quaternion.Euler(0, 0, angle);
        }

        public void Teleport(Transform targetTransform) => ShipHelper.Teleport(transform, targetTransform);

        // TODO: need to be able to disable ship abilities as well for minigames
        public void DisableSkimmer()
        {
            nearFieldSkimmer?.gameObject.SetActive(false);
            farFieldSkimmer?.gameObject.SetActive(false);
        }

        public void SetBoostMultiplier(float multiplier) => boostMultiplier = multiplier;

        public void ToggleGameObject(bool toggle) => gameObject.SetActive(toggle);

        //
        // Attach and Detach
        //
        void Attach(TrailBlock trailBlock) 
        {
            if (trailBlock.Trail != null)
            {
                ShipStatus.Attached = true;
                ShipStatus.AttachedTrailBlock = trailBlock;
            }
        }
    }
}