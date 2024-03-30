mlagents-learn Config/ant284.yaml --run-id=ant284 --force
--resume
--torch-device=cpu

rem mlagents-learn config/ant110.yaml --curriculum=config/curricula/wall-jump/ --run-id=ant110-curriculum --train


rem tensorboard --logdir results --port 6006
