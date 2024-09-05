It is recommended that you start with the tutorial in "/Body Proportions/Scenes/Tutorials".
Please read the document in the path "/Body Proportions/Documentation" for more details.

How to use?
step1:Find your model in hierarchy. select it and add ScalableBonesManager component
step2:Set root in inspector of scalableBonesManager. The root is meaning your root bone.
step3:Click "Build" button. Now you can scale your bones. 

How it works?

ScalableBonesManager:
The ScalableBonesManager collects all the callbacks from the root's children to ensure that the parent ScalableBone updates before the children ScalableBone.  The callbacks are all updated in LateUpdate.
After clicking the "Auto Setup" button, the ScalableBonesManager will reorganise hierarchy of bones , and sets the parent of each child-bone as the root, then adds ScalableBone component for each child.
After clicking the "Recover" button, the ScalableBonesManager will recover hierarchy of bones, and remove ScalableBone components from child-bones.

ScalableBone:
ScalableBones work together. Each ScalableBone moves and rotates with the parent, but does not scale with the parent.

OnlyNew Studio website:
https://www.onlynew.tech
https://assetstore.unity.com/publishers/59523

email:
onlynewstudio@protonmail.com