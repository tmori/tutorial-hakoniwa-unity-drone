#!/bin/bash

ROS2PDU_PATH=../hakoniwa-ros2pdu

CURR_DIR=`pwd`
cd $ROS2PDU_PATH
python utils/create_pdudef.py ${CURR_DIR}/pdudef.csv
cp custom.json ${CURR_DIR}/drone-simulation/custom.json
cp custom.json ${CURR_DIR}/drone-simulation/Assets/Resources/custom.json

cd ${CURR_DIR}
