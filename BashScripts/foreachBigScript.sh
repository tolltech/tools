echo "Converting..."

for f in ./**/*.mov
do
  #echo $f
  if [ "${f}" == "./**/*.mov" ]; then
    continue
  fi
  if [ ! -f "${f}.mp4" ]; then
    echo "Converting ${f}"
    ffmpeg -i $f "${f}.mp4"
    echo "Converted ${f}"
  fi 
done

for f in ./**/*.MOV
do
  #echo $f
  if [ ! -f "${f}.mp4" ]; then
    echo "Converting ${f}"
    ffmpeg -i $f "${f}.mp4"
    echo "Converted ${f}"
  fi 
done

echo "Covers..."

for f in ./**/*.mp4
do
  #echo $f
  if [ ! -f "${f}.cover.jpg" ]; then
    echo "Covering ${f}"
    ffmpegthumbnailer -i "${f}" -o "${f}.cover.jpg"
    echo "Covered ${f}"
  fi 
done

for f in ./**/*.MP4
do
  #echo $f
  if [ ! -f "${f}.cover.jpg" ]; then
    echo "Covering ${f}"
    ffmpegthumbnailer -i "${f}" -o "${f}.cover.jpg"
    echo "Covered ${f}"
  fi 
done

echo "DONE"