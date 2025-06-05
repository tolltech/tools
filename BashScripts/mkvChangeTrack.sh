for f in ./**/*.mkv
do
   echo "-------------"
   echo "Analyse ${f}"
   
   if [ "${f}" == "./**/*.mkv" ]; then
      continue
   fi

   jinfo=$(mkvmerge "$f" -J)
   tracks=$(echo $jinfo | jq .tracks)
   #echo $tracks
   redheadsoundtrack=$(echo $tracks | jq '[.[] | select(.properties.track_name | strings | ascii_downcase | contains("red") and contains("head") and contains("sound"))][0]')
   #echo $redheadsoundtrack

   if [ "$redheadsoundtrack" == "" ]; then
      echo "RedHeadSoundTrack was not found"
      continue
   fi

   replacetracktype=$(echo $redheadsoundtrack | jq -r .type)
   if [ "$replacetracktype" != "audio" ]; then
      echo "TrackType is ${replacetracktype}. It is not audio"
      continue
   fi

   echo "Found RedHeadSoundTrack"
   replacetrackid=$(echo $redheadsoundtrack | jq .id)
   if [ "$replacetrackid" == "" ]; then
      echo "TrackId was not found"
      continue
   fi
   echo "TrackId: ${replacetrackid}"

   if [ "$replacetrackid" == "1" ]; then
      echo "TrackId is already 1. OK"
      continue
   fi

   trackids=$(echo $tracks | jq 'map(.id)')
   #echo $trackids
   newtrackids=()
   error=0
   for i in $trackids
   do
      if [ "$i" == "[" ]; then
         continue;
      fi

      if [ "$i" == "]" ]; then
         continue;
      fi

      reali=$(echo $i | grep -oE '[0-9]+')
      #echo $reali

      currentrack=$(echo $tracks | jq --argjson reali ${reali} '.[] | select(.id==$reali)')
      currenttracktype=$(echo $currentrack | jq -r .type)
      #echo $currenttracktype
      #echo "------------"   
      
      if [ $reali -eq 0 ]; then
         if [ ${currenttracktype} != "video" ]; then
            echo "0 track type is ${currenttracktype}. It is not video"
            $error=1
            break
         fi
      fi

      if [ $reali -eq $replacetrackid ]; then
         newtrackids+=("0:1")
         echo "Replacing ${replacetrackid} with 1 track"
      elif [ $reali -eq 1 ]; then
         if [ $currenttracktype != "audio" ]; then
            echo "Track 1 is ${currenttracktype}. It is not audio"
            $error=1
            break
         fi
         newtrackids+=("0:${replacetrackid}")
      else
         newtrackids+=("0:${reali}")
      fi
   done

   if [ $error -eq 1 ]; then
      continue
   fi

   #echo ${newtrackids[@]}

   delim=""
   joined=""
   for item in "${newtrackids[@]}"; do
   joined="$joined$delim$item"
   delim=","
   done

   #echo $joined
   echo 'Converting'
   mkvmerge --output "tmp.mkv" "(" "${f}" ")" --track-order $joined
   echo 'Converted'
   echo 'Copying'
   mv tmp.mkv "${f}"
   echo 'Copyied'
done

