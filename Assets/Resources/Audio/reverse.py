#!/usr/bin/python

# reverse.py: Reverse all wav files in folder
 
import sys
import wave
import os
from os import listdir
from os.path import isfile, join

#if len(sys.argv) < 3:
#    print "Usage: reverse.py in_file out_file"
#    sys.exit(1)

def reverse_file(in_filename):
	out_filename = in_filename.split('.')[0] + "_rev.wav"
	fin = wave.open("./AudioSamples/" + in_filename,'r')

	write_out_directory = "./ReversedAudioSamples/"

	if not os.path.exists(write_out_directory):
		os.makedirs(write_out_directory)

	file_creator = open(write_out_directory + out_filename, "w+")
	file_creator.close()

	fout = wave.open(write_out_directory + out_filename,'wb')
	fout.setparams([fin.getnchannels(), fin.getsampwidth(), fin.getframerate(), fin.getnframes(), fin.getcomptype(), fin.getcompname()])

	num_frames = fin.getnframes()
	num_channels = fin.getnchannels()

	data = []
	while (fin.tell() < num_frames):
	    frame = fin.readframes(1)
	    data.append(frame)

	data.reverse()

	for frame in data:
	    for sample in frame:
	        fout.writeframesraw(sample)

	fout.close()  
	fin.close()

mypath = "./AudioSamples/"
onlyfiles = [f for f in listdir(mypath) if isfile(join(mypath, f))]

wavfiles = []
for name in onlyfiles:
    if name.endswith(".wav"):
    	# filemaker = open(mypath + "resampled_" + name, "w+")
    	# filemaker.close()
    	os.system("ffmpeg -i ./AudioSamples/{0} -ar 48000 ./AudioSamples/resampled_{0}".format(name))
        wavfiles.append("resampled_" + name)

for wav in wavfiles:
	reverse_file(wav)