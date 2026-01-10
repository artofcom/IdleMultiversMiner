using App.GamePlay.IdleMiner.Common.Types;
using App.GamePlay.IdleMiner.PopupDialog;
using IGCore.MVCS;
using UnityEngine;

public class LobbyScreen : AUnit
{
    [SerializeField] AUnit popupDialog;

    [SerializeField] AUnit gameCardsUnit;

    [ImplementsInterface(typeof(IUnitSwitcher))]
    [SerializeField] MonoBehaviour unitSwitcher;

    IUnitSwitcher UnitSwitcher => unitSwitcher as IUnitSwitcher;

    // DictorMain.Start() -> AUnitSwitcher.Init() -> LobbyScreen.Init()
    public override void Init(AContext ctx)
    {
        base.Init(ctx);

        model = new LobbyScreenModel(context, null);
        controller = new LobbyScreenController(this, view, model, context);

        model.Init();
        controller.Init();

        context.AddData(KeySets.CTX_KEYS.GLOBAL_DLG_KEY, ((PopupDialogUnit)popupDialog).DialogKey);
        popupDialog.Init(ctx);
    }
    public override void Attach() 
    {
        base.Attach();

        gameCardsUnit.Attach();
    }

    public void SwitchUnit(string nextUnit, object data = null)
    {
        UnitSwitcher.SwitchUnit(nextUnit, data);
    }

    public override void Dispose()
    {
        base.Dispose();
        popupDialog.Dispose();
    }
}
