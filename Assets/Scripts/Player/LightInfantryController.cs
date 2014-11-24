using UnityEngine;
using System.Collections;

public class LightInfantryController : PlayerController {
    public override void Start() {
        base.Start();
        runSpeed = 20;
    }

    public override void performClassAction() {
        // Do nothing
    }
}
